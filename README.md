[![N|Solid](https://github.com/BEXIS2/Documents/blob/master/Images/Logo/Logo_BEXIS_rgb_113x28.jpg?raw=true)](http://bexis2.uni-jena.de/) 
# Module PUB

This repo is a PUB for a BEXIS2 module to extend the functionality of the system.

The PUB consists of 1 project and 3 libaries

| Plugin | README |
| ------ | ------ |
| BExIS.Modules.PUB.UI | MVC UI project |
| BEXIS.PUB.Entities | Entities associated with the module |
| BEXIS.PUB.Orm.NH | Contains the nHibernate Mapping files to connect the tables with the entities in the database |
| BEXIS.PUB.Services | In this Libary all managers are deposited, which provide general functionalities for the Entities. eg create, update, delete |


# How to use 

***Precondition:***  Running BEXIS2 Instance in visual Studio

1. Download latest version
2. Create a folder into ***BEXIS2APP\Console\BExIS.Web.Shell\Areas*** and name it like your prefered ***MODULEID*** (only Characters) and copy the downloaded PUB into this folder
    - ..\Console\BExIS.Web.Shell\Areas\\***MODULEID***\BEXIS.Modules.PUB.UI
    - ..\Console\BExIS.Web.Shell\Areas\\***MODULEID***\BEXIS.PUB.Entities
    - ..\Console\BExIS.Web.Shell\Areas\\***MODULEID***\BEXIS.PUB.Orm.NH
    - ..\Console\BExIS.Web.Shell\Areas\\***MODULEID***\BEXIS.PUB.Services
3. Run the ***ModulePUB_Renaming.ps1*** with Power Shell to replace alle **PUB** with **MODULEID** in files and also filenames.
4. Open the BEXIS2 visual studio solution
5. Create a ModuleId folder under the modules folder in the Solution
6. Add the ui project and the libaries to that folder
7. rebuild the BEXIS.MODULEID.Orm.NH project and check wether the mapping files are exitsing in the workspace folder
    - ..\Workspace\Modules\\***MODULEID***\Db\\...
8.  Rebuild the web shell 
9.  Add Module in the catalog Workspace\Modules\Modules.Catalog.xml
```
<?xml version="1.0" encoding="utf-8"?>
<Modules>
  ...
  <Module id="vim" status="active" order="8" />
  <Module id="MODULEID" status="pending" order="1" path="BEXIS.Modules.MODULEID.UI"/>
</Modules>
```
10. Run application
11. After the application is loaded, the status of the module in the module.catalog.xml is set from pending to inactive. change this to active and the module is ready.


