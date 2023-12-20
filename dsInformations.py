import re
import sys # Powershell arguments
import pyodbc # DB Connection
from tkinter import messagebox

class DataInput:
    def __init__(self, name):
        self.name = name
    
    def __str__(self):
        return f"Data Input: {self.name}"

class DataSource:
    def __init__(self, id, name, dataSwitch, connection):
        self.id = id
        self.name = name
        self.dataSwitch = dataSwitch
        self.dataInputs = self.setDataInputs(connection)
    
    def setDataInputs(self, connection):
        result = []

        if connection:   
            sql_query = "SELECT [INPUT_NAME] FROM [DATA_INPUT] JOIN [INPUT_BLOCK_ASS] ON [DATA_INPUT].[INPUT_ID] = [INPUT_BLOCK_ASS].[INPUT_ID] WHERE [SOURCE_ID] = '" + str(self.id) + "' GROUP BY [DATA_INPUT].[INPUT_ID], [DATA_INPUT].[INPUT_NAME]"

            cursor = connection.cursor()
            cursor.execute(sql_query)

            for row in cursor.fetchall():
                dataInputName = str(row[0]).replace("'", '').replace(',', '').replace(' ', '')
                dataInput = DataInput(dataInputName)
                result.append(dataInput)
        return result
    
    def __str__(self):
        return f"Data Source: {self.id}, {self.name}, {self.dataSwitch}, {self.dataInputs.__str__()}"

class Equipment:
    def __init__(self, operantId, operant, operantType):
        self.operantId = operantId
        self.operant = operant
        self.operantType = operantType

class Environment:
    def __init__(self, siteId, name, server, database, storedProcedure, storedProcedureServer, storedProcedureDatabase, level):
        self.siteId = siteId
        self.name = name
        self.server = server
        self.database = database
        self.storedProcedure = storedProcedure
        self.storedProcedureServer = storedProcedureServer
        self.storedProcedureDatabase = storedProcedureDatabase
        self.level = level
        self.connection = establish_connection(self.server, self.database)
        self.connectionSP = establish_connection(self.storedProcedureServer, self.storedProcedureDatabase)
        self.equipments = self.setEquipments()
        #self.equipments = None
        self.dataSources = []
    
    def getEquipments(self):
        return self.equipments
    
    def setEquipments(self):
        result = []

        if self.connection:
            sql_query = "SELECT * FROM " + self.database + ".[dbo].[OPERGROUPES] o join " + self.database + ".[dbo].[OPERGROUPES_LEVEL] l on o.OPERANT_ID = l.OPERANT_ID and o.SITE_ID = '" + str(self.siteId) + "' and o.SITE_ID = l.SITE_ID where l.OPERANT_LEVEL = '" + str(self.level) + "';"

            cursor = self.connection.cursor()
            cursor.execute(sql_query)

            for row in cursor.fetchall():
                equipment = None
                operantId = row[1]
                operant = row[2]
                operantType = row[3]
                if operantType >= 0:
                    equipment = Equipment(operantId, operant, operantType)
                    result.append(equipment)
                else:
                    if self.connectionSP:
                        sql_query = "EXEC " + self.storedProcedure + " @Site_Id = '" + self.siteId + "', @Source_Name = 'ALL_LIST_EQUIPMENT', @Data_Req = '" + operant + "';"
                        
                        try:
                            cursor2 = self.connectionSP.cursor()
                            cursor2.execute(sql_query)

                            for row2 in cursor2.fetchall():
                                equipment = Equipment(row2[3], row2[0], row2[2])
                                result.append(equipment)
                        except pyodbc.Error as ex:
                            if len(ex.args) == 1:
                                message = "Error retrieving equipment: " + ex.args[0]
                            elif len(ex.args) == 2:
                                message = "Error retrieving equipment: " + ex.args[1]
                            messagebox.showwarning(title="Error", message=message)
        return result
    
    def getDataInputs(self):
        result = []
        for dataSource in self.dataSources:
            result += dataSource.dataInputs
        return result
    
    def getDataSources(self):
        return self.dataSources
    
    def setDataSource(self, dataSourceName, dataSwitch):
        if self.connection:
            sql_query = "SELECT [SOURCE_ID], [SOURCE_NAME], [DATA_SWITCH] FROM [DATA_SOURCE] WHERE [SOURCE_NAME] = '" + dataSourceName + "' AND [DATA_SWITCH] = '" + dataSwitch + "'"

            cursor = self.connection.cursor()
            cursor.execute(sql_query)

            for row in cursor.fetchall():
                dataSource = DataSource(row[0], dataSourceName, dataSwitch, self.connection)
                self.dataSources.append(dataSource)
    
    def getSelfData(self):
        dataReq = ""
        data = []
        for dataSource in self.dataSources:
            for dataInput in dataSource.dataInputs:
                tmpDataReq = str(dataInput.name)
                if "(" in tmpDataReq:
                    # Remove _
                    tmpDataReq = tmpDataReq.replace('_', '')
                # Remove everything between () & []
                tmpDataReq = re.sub("[\(\[].*?[\)\]]", "", tmpDataReq)
                dataReq += "," + tmpDataReq

            if(len(dataReq) > 0):
                sql_query = "EXEC " + self.storedProcedure + " @Site_Id = '" + self.siteId + "', @Source_Name = '" + dataSource.name +"', @Data_Req = '" + dataReq + "';"

                if self.connectionSP:
                    cursor = self.connectionSP.cursor()
                    try:
                        cursor.execute(sql_query)
                    except pyodbc.Error as ex:
                        message = "Error retrieving data, there is a problem with the stored procedure: " + ex.args[1]
                        messagebox.showwarning(title="Error", message=message)

                    for row in cursor.fetchall():
                        data.append(row)
                return data
        message = "No data found"
        messagebox.showwarning(title="Error", message=message)
        return False
    
    def getData(self, dataSource, dataSwitch, dataInputs, equipments):
        dataReq = ""
        data = []
        for dataInput in dataInputs:
            tmpDataReq = dataInput
            if "(" in tmpDataReq:
                # Remove _
                tmpDataReq = tmpDataReq.replace('_', '')
                # Remove everything between () & []
            tmpDataReq = re.sub("[\(\[].*?[\)\]]", "", tmpDataReq)
            dataReq += "," + tmpDataReq

        if(len(dataReq) > 0):
            for equipment in equipments:

                if self.storedProcedure == "dbo.GetPatternDetectionSpecificPotDataV2DEV" or self.storedProcedure == "dbo.GetPatternDetectionSpecificPotDataV2":
                    sql_query = "EXEC " + self.storedProcedure + " @Site_Id = '" + self.siteId + "', @Source_Name = '" + dataSource +"', @Data_Req = '" + dataReq + "', @Single_Pot = '" + equipment + "', @DATA_SWITCH = '" + dataSwitch + "';"
                else:
                    sql_query = "EXEC " + self.storedProcedure + " @Site_Id = '" + self.siteId + "', @Source_Name = '" + dataSource +"', @Data_Req = '" + dataReq + "', @Single_Equi = '" + equipment + "', @DATA_SWITCH = '" + dataSwitch + "';"
        
                if self.connectionSP:
                    cursor = self.connectionSP.cursor()
                    try:
                        cursor.execute(sql_query)

                        try:
                            if cursor.description is not None:
                                row = []
                                for column in cursor.description:
                                    row.append(column[0])
                                data.append(row)

                            for row in cursor.fetchall():
                                data.append(row)
                        except pyodbc.Error as ex:
                            if len(ex.args) == 1:
                                message = "No data found: " + ex.args[0]
                            elif len(ex.args) == 2:
                                message = "No data found: " + ex.args[1]
                            messagebox.showwarning(title="Error", message=message)
                            break
                    except pyodbc.Error as ex:
                        if len(ex.args) == 1:
                            message = "Error retrieving data, or there is a problem with the stored procedure: " + ex.args[0]
                        elif len(ex.args) == 2:
                            message = "Error retrieving data, or there is a problem with the stored procedure: " + ex.args[1]
                        messagebox.showwarning(title="Error", message=message)
                        break     
            return data
        return False

    def __str__(self):
        return f"Environment: {self.siteId}, {self.name}, {self.server}, {self.database}, {self.storedProcedure}, {self.storedProcedureServer}, {self.storedProcedureDatabase}, {self.level}, {self.dataSources.__str__()}"

def instantiateEnvironment(args):
    if len(args) == 6:
        siteId = args[1]
        environmentName = args[2]
        dataSourceName = args[3]
        dataSwitch = args[4]
        level = args[5]

        connection = establish_connection("casagzsql5", "COA")
        if connection:
            sql_query = "SELECT [ptn].[APP_MODE].[NAME], [ptn].[SERVER].[INSTANCE], [ptn].[DATABASE].[DB_NAME], [ptn].[SP_DATA].[VALUE] FROM [ptn].[APP_MODE] JOIN [ptn].[DATABASE] ON [ptn].[APP_MODE].[REF_DB_ID] = [ptn].[DATABASE].[ID] JOIN [ptn].[SERVER] ON [ptn].[DATABASE].[REF_SERVEUR_ID] = [ptn].[SERVER].[ID] JOIN [ptn].[SP_DATA] ON [ptn].[APP_MODE].[REF_SP_DATA_ID] = [ptn].[SP_DATA].[ID] WHERE [ptn].[APP_MODE].[NAME] = '" + environmentName + "' AND [ptn].[APP_MODE].[ENABLE] = 'True';"
            cursor = connection.cursor()
            cursor.execute(sql_query)

            for row in cursor.fetchall():
                sql_query = "EXEC [ptn].[Get_ExeMode_ConnectionString];"
                cursor2 = connection.cursor()
                cursor2.execute(sql_query)

                for row2 in cursor2.fetchall():
                    if row2[0] == row[0] and row2[2] == "DATA":
                        environment = Environment(siteId, environmentName, row[1], row[2], row[3], row2[5], row2[6], level)
                        environment.setDataSource(dataSourceName, dataSwitch)
        return environment
    else:
        message = "Erreur, 1 ou plusieurs arguments manquant pour l'execution de dataSourceInformations"
        messagebox.showwarning(title="Error", message=message)
        return False

def establish_connection(server, database):
    try:
        connection_str = (
            f'TRUSTED_CONNECTION={{yes}};' # Setting trusted_connection to yes enable the connection via windows authentication
            f'DRIVER={{ODBC Driver 17 for SQL Server}};'
            f'SERVER={server};'
            f'DATABASE={database};'
        )
        conn = pyodbc.connect(connection_str)
    except pyodbc.Error as ex:
        return False
    return conn

def Main():
    global environment
    #environment = instantiateEnvironment(['dataSourceInformations', '336', 'PROD', 'RED_POT_DIM', '0', '0'])
    environment = instantiateEnvironment(sys.argv)
    print(environment.__str__())

if __name__ == "__main__":
    environment = None
    Main()