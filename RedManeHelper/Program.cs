﻿
using DemoKatan.Demos;
using DemoKatan.Entities;

//var localConfigs = @"C:\Users\jreiner\Downloads\77c4e6a9-3bf1-497d-a916-00e12ccad110";

//var configRepoLoc = @"C:\Users\jreiner\source\repos\AR-mCase-Config\Datalists";

//var c = new CompareSysNames();

//c.Compare(localConfigs, configRepoLoc);
//var url = @"https://dev.azure.com/RedMane-mCase-AR-CCWIS/AR%20Development/_build?definitionId=90";


//var t = new TaskWhenAll();

//await t.ThrowIfCancelled();


//var x = new InvokeEvent();

//x.Run();


var factory = new GenerateC_ObjectFromJson();

factory.EntityFactory();