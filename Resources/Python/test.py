
# Move one particular unit
unit = scenario["OOB_Fr_Antoine_Drouot"]
unit.position.south = 100;
print scenario.UnitCsvLine(unit, scenario.scenarioHeaders);

#move a unit's children
for echelon in scenario["OOB_Fr_Joseph_Christiani"].echelon.children:
	unit = echelon.unit
	unit.position.south += 100;
	print scenario.UnitCsvLine(unit, scenario.scenarioHeaders);



    


