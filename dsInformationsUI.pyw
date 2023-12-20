import sys # For arguments
import tkinter as tk # For UI
import tkinter.scrolledtext
from tkinter.constants import *
from tkinter import *
from tkinter import messagebox
from tkinter import ttk
import tkinter.font as TkFont # Text font for envText
import dsInformations as dsi

def setDatagrid():
    global window
    global frame
    data = getDSData()

    if data:
        datagridTextArea = tk.scrolledtext.ScrolledText(frame, bg="SystemButtonFace", wrap="none")
        datagridTextArea.grid(column=4, row=0, rowspan=8, sticky='ns')

        for i in range(len(data)):
            for j in range(len(data[i])):
                e = Entry(datagridTextArea, relief=GROOVE)
                e.grid(row=i, column=j, sticky=NSEW)
                e.insert(END, data[i][j])
                datagridTextArea.window_create("end", window=e)
                datagridTextArea.insert("end", "")
            datagridTextArea.insert("end", "\n")

def getDSData():
    for dataSource in environment.dataSources:
        data = environment.getData(dataSource.name, dataSource.dataSwitch, selectedDataInputs, selectedEquipments)
    return data

def checkUnCheckAll(typeGrid, check):
    if typeGrid == "equipment":
        for equipment in equipmentsCheckbox:
            if check:
                equipment.set(True)
            else:
                equipment.set(False)
    elif typeGrid == "dataInput":
        for dataInput in dataInputsCheckbox:
            if check:
                dataInput.set(True)
            else:
                dataInput.set(False)
    getSelection()

def update_textWidget_size(textWidgets):
    width=0
    lines=0
    if len(textWidgets) > 1:
        for textWidget in textWidgets:
            for line in textWidget.get("1.0", "end-1c").split("\n"):
                tmp_width=max(width,TkFont.Font().measure(line))
                if tmp_width > width:
                    width = tmp_width
        for textWidget in textWidgets:
            lines = 0
            for line in textWidget.get("1.0", "end-1c").split("\n"):
                lines += 1
            textWidget.config(height=lines, width=round(width/7))
    else:
        for line in textWidget.get("1.0", "end-1c").split("\n"):
            width=max(width,TkFont.Font().measure(line))
            lines += 1
        textWidget.config(height=lines, width=round(width/7))

def getSelection():
    global selectedEquipments
    global selectedDataInputs

    selectedEquipments = []
    selectedDataInputs = []

    for idx, equipment in enumerate(equipmentsCheckbox):
        if equipment.get() == 1:
            selectedEquipments.append(equipments[idx].operant)
    
    for idx, dataInput in enumerate(dataInputsCheckbox):
        if dataInput.get() == 1:
            selectedDataInputs.append(dataInputs[idx].name)

def initializeWindow():
    global window
    global frame
    window = Tk()
    window.title("Data Source Informations")
    window.grid()
    window.grid_rowconfigure(0, weight=1)
    window.grid_columnconfigure(0, weight=1)
    frame = ttk.Frame(window, padding=10)
    frame.grid()
    frame.grid_rowconfigure(0, weight=1)
    frame.grid_columnconfigure(0, weight=1)

# /************* INFO *************\
    envNameText = Text(frame)
    envNameText.insert(1.0, "Env: " + environment.name)
    envNameText.grid(column=0, row=0)
    envNameText.configure(state="disabled")
    envNameText.configure(inactiveselectbackground=envNameText.cget("selectbackground"))

    envSiteText = Text(frame)
    envSiteText.insert(1.0, "Site: " + str(environment.siteId))
    envSiteText.grid(column=0, row=1)
    envSiteText.configure(state="disabled")
    envSiteText.configure(inactiveselectbackground=envSiteText.cget("selectbackground"))

    envLevelText = Text(frame)
    envLevelText.insert(1.0, "Level: " + str(environment.level))
    envLevelText.grid(column=0, row=2)
    envLevelText.configure(state="disabled")
    envLevelText.configure(inactiveselectbackground=envLevelText.cget("selectbackground"))

    envServerText = Text(frame)
    envServerText.insert(1.0, "Serveur: " + environment.server)
    envServerText.grid(column=0, row=3)
    envServerText.configure(state="disabled")
    envServerText.configure(inactiveselectbackground=envServerText.cget("selectbackground"))

    envDatabaseText = Text(frame)
    envDatabaseText.insert(1.0, "Base de données: " + environment.database)
    envDatabaseText.grid(column=0, row=4)
    envDatabaseText.configure(state="disabled")
    envDatabaseText.configure(inactiveselectbackground=envDatabaseText.cget("selectbackground"))

    envStoredProcedureText = Text(frame)
    envStoredProcedureText.insert(1.0, "Procédure Stockée: " + environment.storedProcedure)
    envStoredProcedureText.grid(column=0, row=5)
    envStoredProcedureText.configure(state="disabled")
    envStoredProcedureText.configure(inactiveselectbackground=envStoredProcedureText.cget("selectbackground"))

    envStoredProcedureServerText = Text(frame)
    envStoredProcedureServerText.insert(1.0, "Serveur Procédure Stockée: " + environment.storedProcedureServer)
    envStoredProcedureServerText.grid(column=0, row=6)
    envStoredProcedureServerText.configure(state="disabled")
    envStoredProcedureServerText.configure(inactiveselectbackground=envStoredProcedureServerText.cget("selectbackground"))

    envStoredProcedureDatabaseText = Text(frame)
    envStoredProcedureDatabaseText.insert(1.0, "Base de données Procécudre Stockée: " + environment.storedProcedureDatabase)
    envStoredProcedureDatabaseText.grid(column=0, row=7)
    envStoredProcedureDatabaseText.configure(state="disabled")
    envStoredProcedureDatabaseText.configure(inactiveselectbackground=envStoredProcedureDatabaseText.cget("selectbackground"))

    envDataSourceDescriptionText = Text(frame)
    envDataSourceDescriptionText.insert(1.0, "Description Source de Données: ")
    envDataSourceDescriptionText.grid(column=0, row=8)
    envDataSourceDescriptionText.configure(state="disabled")
    envDataSourceDescriptionText.configure(inactiveselectbackground=envDataSourceDescriptionText.cget("selectbackground"))

    envDataSourceSQLQueryText = Text(frame)
    envDataSourceSQLQueryText.insert(1.0, "SQL Query: ")
    envDataSourceSQLQueryText.grid(column=0, row=9)
    envDataSourceSQLQueryText.configure(state="disabled")
    envDataSourceSQLQueryText.configure(inactiveselectbackground=envDataSourceSQLQueryText.cget("selectbackground"))

    update_textWidget_size([envNameText, envSiteText, envLevelText, envServerText, envDatabaseText, envStoredProcedureText, envStoredProcedureServerText, envStoredProcedureDatabaseText, envDataSourceDescriptionText, envDataSourceSQLQueryText])

# /************* EQUIPMENT AND DATA INPUT *************\

# /**** EQUIPMENT ****\

    ttk.Label(frame, text="Équipements:").grid(column=1, row=0)

    equipmentCheckAllBtn = ttk.Button(frame, text="Cocher tout", command=lambda: checkUnCheckAll("equipment", True))
    equipmentCheckAllBtn.grid(column=1, row=1)
    equipmentUncheckAllBtn = ttk.Button(frame, text="Décocher tout", command=lambda: checkUnCheckAll("equipment", False))
    equipmentUncheckAllBtn.grid(column=2, row=1)

    global equipmentsCheckbox
    equipmentTextArea = tk.scrolledtext.ScrolledText(frame, bg="SystemButtonFace")
    equipmentTextArea.grid(column=1, row=2, columnspan=2)

    for equipment in equipments:
        var = tk.IntVar()
        equipmentsCheckbox.append(var)
        checkBtn = tk.Checkbutton(equipmentTextArea, text=equipment.operant, variable=var, command=getSelection)
        equipmentTextArea.window_create("end", window=checkBtn)
        equipmentTextArea.insert("end", "\n")
    
    equipmentTextArea.configure(state=DISABLED, cursor='')


# /**** DATA INPUT ****\

    ttk.Label(frame, text="Data Inputs:").grid(column=1, row=3)

    dataInputCheckAllBtn = ttk.Button(frame, text="Cocher tout", command=lambda: checkUnCheckAll("dataInput", True))
    dataInputCheckAllBtn.grid(column=1, row=4)
    dataInputUncheckAllBtn = ttk.Button(frame, text="Décocher tout", command=lambda: checkUnCheckAll("dataInput", False))
    dataInputUncheckAllBtn.grid(column=2, row=4)

    global dataInputsCheckbox
    dataInputTextArea = tk.scrolledtext.ScrolledText(frame, bg="SystemButtonFace")
    dataInputTextArea.grid(column=1, row=5, columnspan=2, rowspan=3)

    for dataInput in dataInputs:
        var = tk.IntVar()
        dataInputsCheckbox.append(var)
        checkBtn = tk.Checkbutton(dataInputTextArea, text=dataInput.name, variable=var, command=getSelection)
        dataInputTextArea.window_create("end", window=checkBtn)
        dataInputTextArea.insert("end", "\n")
    
    dataInputTextArea.configure(state=DISABLED, cursor='')

    scanBtn = ttk.Button(frame, text="Scan", command=setDatagrid)
    scanBtn.grid(column=1, row=8)

# /************* DATAGRID *************\



def getEnvData(testMode):
    global environment
    global equipments
    global dataInputs

    if testMode:
        environment = dsi.instantiateEnvironment(['dataSourceInformations', '336', 'PROD', 'RED_POT_DIM', '0', '0'])
        #environment = dsi.instantiateEnvironment(['dataSourceInformations', '336', 'CAST_SUPRV_QA', 'SOURCEPI_1', '0', '0'])
        #environment = dsi.instantiateEnvironment(['dataSourceInformations', '336', 'CAST_SUPRV_QA', 'ECHANTILLON_FACT', '0', '0'])
        equipments = environment.getEquipments()
        dataInputs = environment.getDataInputs()
    else:
        environment = dsi.instantiateEnvironment(sys.argv)
        equipments = environment.getEquipments()
        dataInputs = environment.getDataInputs()

def Main():
    getEnvData(True)
    initializeWindow()

if __name__ == "__main__":
    environment = None
    window = None
    frame = None
    equipments = []
    equipmentsCheckbox = []
    selectedEquipments = []
    dataInputs = []
    dataInputsCheckbox = []
    selectedDataInputs = []
    dsData = None
    Main()
    window.mainloop()