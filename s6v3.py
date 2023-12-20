import subprocess as sp
import os
import os.path
from pathlib import Path
from enum import Enum
import tkinter as tk
from tkinter import *
from tkinter import messagebox
from tkinter import ttk
import tkinter.font as TkFont # Text font for envInfoText
import pyodbc # DB Connection
import shutil # Copy file
import config as cfg
import colorama
from colorama import Fore

class DataInput:
    def __init__(self, name):
        self.name = name

class DataSource:
    def __init__(self, sourceId, name, dataSwitch, connection):
        self.sourceId = str(sourceId)
        self.name = name
        self.dataSwitch = str(dataSwitch)
        self.dataInputs = self.getDataInputs(connection)
    
    def getDataInputs(self, connection):
        result = []
        connection_error_nbr = 0

        if connection:   
            if cfg.get_only_used_data_input:
                sql_query = "SELECT [INPUT_NAME] FROM [DATA_INPUT] JOIN [INPUT_BLOCK_ASS] ON [DATA_INPUT].[INPUT_ID] = [INPUT_BLOCK_ASS].[INPUT_ID] WHERE [SOURCE_ID] = '" + self.sourceId + "' GROUP BY [DATA_INPUT].[INPUT_ID], [DATA_INPUT].[INPUT_NAME]"
            else:
                sql_query = "SELECT [INPUT_NAME] FROM [DATA_INPUT] WHERE [SOURCE_ID] = '" + self.sourceId + "'"

            cursor = connection.cursor()
            cursor.execute(sql_query)

            for row in cursor.fetchall():
                dataInputName = str(row[0]).replace("'", '').replace(',', '').replace(' ', '')
                dataInput = DataInput(dataInputName)
                result.append(dataInput)
        else:
            connection_error_nbr += 1  
        return result


class Environment:
    def __init__(self, name, server, database, storedProcedure, storedProcedureServer, storedProcedureDatabase):
        self.name = name
        self.server = server
        self.database = database
        self.storedProcedure = storedProcedure
        self.storedProcedureServer = storedProcedureServer
        self.storedProcedureDatabase = storedProcedureDatabase
        self.dataSources = self.getDataSources()
        self.data_source_count = len(self.dataSources)
    
    def getDataSources(self):
        result = []
        connection_error_nbr = 0

        connection = establish_connection(self.server, self.database, True)
        if connection:
            sql_query = "SELECT [SOURCE_ID], [SOURCE_NAME], [DATA_SWITCH] FROM [DATA_SOURCE] ORDER BY [SOURCE_NAME] ASC"

            cursor = connection.cursor()
            cursor.execute(sql_query)

            for row in cursor.fetchall():
                dataSourceName = str(row[1]).replace('(', '').replace(')', '').replace("'", '').replace(',', '').replace(' ', '')
                dataSource = DataSource(row[0], dataSourceName, row[2], connection)
                result.append(dataSource)
            
            print(Fore.LIGHTCYAN_EX + "Env: " + self.name + " is loaded with server: " + self.server + " on database: " + self.database)
            print("----------------------------------------------------------------------------")
            print(Fore.RESET)
        else:
            connection_error_nbr += 1
        return result

    def to_str(self):
        return "Env: " + self.name + " in the server: " + self.server + " using the database: " + self.database + " with the stored procedure: " + self.storedProcedure + " found in the server: " + self.storedProcedureServer + " in the database: " + self.storedProcedureDatabase

    def to_info(self):
        return "Env: " + self.name + "\nServer: " + self.server + "\nDatabase: " + self.database + "\nStored procedure: " + self.storedProcedure + "\nStored procedure server: " + self.storedProcedureServer + "\nStore procedure database: " + self.storedProcedureDatabase

def createEnvList():
    envList = []
    connection = establish_connection(cfg.pattern["server"], cfg.pattern["database"], True)
    if connection and cfg.force_use_config_file_envs == False:
        sql_query = "SELECT [ptn].[APP_MODE].[NAME], [ptn].[SERVER].[INSTANCE], [ptn].[DATABASE].[DB_NAME], [ptn].[SP_DATA].[VALUE] FROM [ptn].[APP_MODE] JOIN [ptn].[DATABASE] ON [ptn].[APP_MODE].[REF_DB_ID] = [ptn].[DATABASE].[ID] JOIN [ptn].[SERVER] ON [ptn].[DATABASE].[REF_SERVEUR_ID] = [ptn].[SERVER].[ID] JOIN [ptn].[SP_DATA] ON [ptn].[APP_MODE].[REF_SP_DATA_ID] = [ptn].[SP_DATA].[ID] WHERE [ptn].[APP_MODE].[ENABLE] = 'True' ORDER BY [ptn].[APP_MODE].[NAME] ASC;"
        cursor = connection.cursor()
        cursor.execute(sql_query)

        for row in cursor.fetchall():
            sql_query = "EXEC [ptn].[Get_ExeMode_ConnectionString];"
            cursor2 = connection.cursor()
            cursor2.execute(sql_query)

            for row2 in cursor2.fetchall():
                if row2[0] == row[0] and row2[2] == "DATA":
                    envList.append(Environment(row[0], row[1], row[2], row[3], row2[5], row2[6]))
    else: # Use the config file if can't connect to the main server to get the environments
        for env in cfg.environments:
            if env["load"]:
                envList.append(Environment(env["name"], env["server"], env["database"], env["storedProcedure"], env["storedProcedureServer"], env["storedProcedureDatabase"]))

    return envList

def establish_connection(server, database, debug):
    if debug:
        print('Establishing DB connection...')
    try:
        connection_str = (
            f'TRUSTED_CONNECTION={{yes}};' # Setting trusted_connection to yes enable the connection via windows authentication
            f'DRIVER={{ODBC Driver 17 for SQL Server}};'
            f'SERVER={server};'
            f'DATABASE={database};'
        )
        conn = pyodbc.connect(connection_str)
        if debug:
            print(Fore.GREEN + "Successfully connected to server " + server + " on database " + database)
    except pyodbc.Error as ex:
        if debug:
            print(Fore.RED + "Error, couldn't connect to server " + server + " or database " + database)
            #sqlMessage = ex.args[1] # 0 would be only the error number
            #print(sqlMessage)
            print("----------------------------------------------------------------------------")
            print(Fore.RESET)
        return False
    if debug:
        print(Fore.RESET)
    return conn

def openReport():
    try:
        os.startfile(fileName)
    except:
        message = "The report for the Data Source: " + dataSourceDropdownValue.get() + " doesn't exist in the environment: " + envDropdownValue.get() + " Path: " + fileName
        messagebox.showwarning(title="Error", message=message)
    return None

def onEnvDropdownChanged(value):
    changeDataSourceList()
    changeEnvInfoText()
    changeDataSourceCountText()
    onDataSourceDropdownChanged(None)
    return None

def onDataSourceDropdownChanged(value):
    setCurrentDataSource()
    checkIfFileExist()
    return None

def changeDataSourceList():
    global currentEnv
    dataSources.clear()
    for env in envDataSourceList:
        if env.name == envDropdownValue.get():
            currentEnv = env
            for dataSource in env.dataSources:
                if not isinstance(dataSource, Environment): # Used to remove the Environment Object at the beginning
                    dataSources.append(dataSource.name)

    dataSourceDropdownValue.set( dataSources[0] )
    dataSourceDropdown["values"] = dataSources

def setCurrentDataSource():
    global fileName
    global currentDataSource
    fileName = filePath + envDropdownValue.get() + "\\" + dataSourceDropdownValue.get() + fileExtension
    for dataSource in currentEnv.dataSources:
        if dataSource.name == dataSourceDropdownValue.get():
            currentDataSource = dataSource

def checkIfFileExist():
    if os.path.isfile(fileName):
        print("Report found")
        switchCreateReportBtnState(False)
        openReportBtn.config(state=NORMAL)
    else:
        print("Report not found")
        switchCreateReportBtnState(True)
        openReportBtn.config(state=DISABLED)

def switchCreateReportBtnState(enable):
    if enable:
        if doesCurrentDataSourceValueExist():
            createReportBtn.config(state=NORMAL)
        else:
            if cfg.authorize_non_related_report_creation:
                createReportBtn.config(state=NORMAL)
            else:
                createReportBtn.config(state=DISABLED)
    else:
        createReportBtn.config(state=DISABLED)

def changeDataSourceCountText():
    dataSourceCountLabel.config(text = currentEnv.data_source_count)

def getDataSourceData():
    global currentDataGrid

    currentDataGrid = []

    connection = establish_connection(currentEnv.storedProcedureServer, currentEnv.storedProcedureDatabase, False)
    if connection:
        dataReq = ""
        for dataInput in currentDataSource.dataInputs:
            tmpDataReq = str(dataInput.name)
            if "(" in tmpDataReq:
                # Remove _
                tmpDataReq = tmpDataReq.replace('_', '')
            # Remove everything between () & []
            tmpDataReq = re.sub("[\(\[].*?[\)\]]", "", tmpDataReq)
            dataReq += "," + tmpDataReq
        if(len(dataReq) > 0):
            sql_query = "EXEC " + currentEnv.storedProcedure + " @Site_Id = '398', @Source_Name = '" + currentDataSource.name +"', @Data_Req = '" + dataReq + "';"
    
            cursor = connection.cursor()
            try:
                cursor.execute(sql_query)
            except:
                message = "Error retrieving data, there is a problem with the stored procedure"
                messagebox.showwarning(title="Error", message=message)

            for row in cursor.fetchall():
                currentDataGrid.append(row)

def displayDatagrid():

        getDataSourceData()

        window = Tk()
        window.title(currentDataSource.name + " Data Visualizer")
        window.grid()
        window.grid_rowconfigure(0, weight=1)
        window.grid_columnconfigure(0, weight=1)
        frm = ttk.Frame(window, padding=10)
        frm.grid()
        frm.grid_rowconfigure(0, weight=1)
        frm.grid_columnconfigure(0, weight=1)
        ttk.Label(frm, text="Donnée:").grid(column=0, row=0)
        
        rows = []
        for i in range(len(currentDataGrid)):
            cols = []
            for j in range(len(currentDataGrid[i])):
                e = Entry(frm, relief=GROOVE)
                e.grid(row=i, column=j, sticky=NSEW)
                e.insert(END, currentDataGrid[i][j])
                cols.append(e)
            rows.append(cols)

def createReport():
    dst_dir= filePath + envDropdownValue.get() + "\\" + dataSourceDropdownValue.get() + fileExtension
    shutil.copy(templatePath,dst_dir)
    checkIfFileExist()

def changeEnvInfoText():
    envInfoText.config(state=NORMAL)
    envInfoText.delete(1.0, "end")
    envInfoText.insert(1.0, currentEnv.to_info())
    update_textWidget_size(envInfoText)
    envInfoText.config(state=DISABLED)

def update_textWidget_size(textWidget):
    width=0
    lines=0
    for line in textWidget.get("1.0", "end-1c").split("\n"):
        width=max(width,TkFont.Font().measure(line))
        lines += 1
    textWidget.config(height=lines, width=round(width/7))
    #textWidget.place(width=width+10)

def doesCurrentDataSourceValueExist():
    for dataSource in dataSources:
        if dataSourceDropdownValue.get() == dataSource:
            return True
    return False

filePath = cfg.files["reportsFolderPath"]
fileExtension = cfg.files["fileExtension"]
templatePath = cfg.files["templatePath"]
fileName = ""
currentEnv = None
currentDataSource = None
currentDataGrid = None
colorama.init()

window = Tk()
window.title("Data Source Report Openner")
window.grid()
window.grid_rowconfigure(0, weight=1)
window.grid_columnconfigure(0, weight=1)
frm = ttk.Frame(window, padding=10)
frm.grid()
frm.grid_rowconfigure(0, weight=1)
frm.grid_columnconfigure(0, weight=1)
ttk.Label(frm, text="Environnement:").grid(column=0, row=0)
ttk.Label(frm, text="Source de donnée:").grid(column=0, row=1)
dataSourceCountLabel = ttk.Label(frm, text="")
dataSourceCountLabel.grid(column=0, row=2)

# Create the Environment List
envDataSourceList = createEnvList()
# Get only the environments names
envsName = []
for env in envDataSourceList:
    envsName.append(env.name)

# Variable to keep track of the option in the envDropdown
envDropdownValue = StringVar(window)

# Variable to keep track of the option in the dataSourceDropdown
dataSourceDropdownValue = StringVar(window)

# Dropdown grid location must be placed on a second line, otherwise envDropdown will never be an object of the dropdown, but one if the grid
# https://stackoverflow.com/questions/1101750/why-do-i-get-attributeerror-nonetype-object-has-no-attribute-using-tkinter-w

# Create the environment Dropdown
envDropdown = ttk.OptionMenu(frm, envDropdownValue, envsName[0], *envsName, command=onEnvDropdownChanged) # Frame, var in which the value will be stocked, default value, list for the options, command
envDropdown.grid(column=1, row=0)

# Load the Data sources for the current Environment (Copy the behaviour of changeDataSourceList())
dataSources = []
for env in envDataSourceList:
    if env.name == envDropdownValue.get():
        for dataSource in env.dataSources:
            dataSources.append(dataSource.name)

# Create the Data Source Dropdown
dataSourceDropdown = ttk.Combobox(frm, textvariable=dataSourceDropdownValue, values=dataSources)
dataSourceDropdown.grid(column=1, row=1)
dataSourceDropdown.bind("<<ComboboxSelected>>", onDataSourceDropdownChanged)
# Manage the keyboard input for Enter (After someone enters something in the text zone and press ENTER)
dataSourceDropdown.bind("<Return>", onDataSourceDropdownChanged)
# Manage the keyboard input for any key, so it check each time a new character is added or removed from the field
dataSourceDropdown.bind('<KeyRelease>', onDataSourceDropdownChanged)
# Set the initial text for the dataSourceDropdown
dataSourceDropdownValue.set( dataSources[1] )

envInfoText = Text(frm)
envInfoText.insert(1.0, "ENV INFO TEXT")
envInfoText.grid(column=2, row=0)
envInfoText.configure(state="disabled")
envInfoText.configure(inactiveselectbackground=envInfoText.cget("selectbackground"))

createReportBtn = ttk.Button(frm, text="Créer le rapport", command=createReport)
createReportBtn.config(state=DISABLED)
createReportBtn.grid(column=2, row=1)

openReportBtn = ttk.Button(frm, text="Lancer le rapport", command=openReport)
openReportBtn.config(state=DISABLED)
openReportBtn.grid(column=1, row=2)

openDatagridBtn = ttk.Button(frm, text="Visualier les données", command=displayDatagrid)
openDatagridBtn.grid(column=1, row=3)

onEnvDropdownChanged(None)

ttk.Button(frm, text="Quit", command=window.destroy).grid(column=1, row=4)
window.mainloop()