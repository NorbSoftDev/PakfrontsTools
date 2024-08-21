'''Place anything above regiment in a random position in a quadrant'''

mapAreas = Utils.MapTools.GetGrids(scenario.map, 2, 2)
print "mapAreas",mapAreas
print "mapAreas[0,0]",mapAreas[0,0]
print "mapAreas[0,0].name",mapAreas[0,0].name
scenario.map.Load()

for g in scenario.map.grayscales:
    if g is None:
        continue
    print g.grayscale,g.id,g.movementFactor,g.isRoad

bitmap = Utils.MapTools.GetBitmap(scenario.map);

for echelon in selectedEchelons[0].AllDescendants():
    if echelon.rank < SOW.ERank.Brigade:
        # skip regiments and battalions
	    continue

    unit = echelon.unit
    if unit is None:
        continue
		
    position = mapAreas[0,0].GetRandomPosition();
	
    unit.transform.SetPosition( mapAreas[0,0].GetRandomPosition() );
    unit.transform.AimAt( mapAreas[0,0].GetRandomPosition() ) ;
    print unit.transform
    if echelon.rank == SOW.ERank.Brigade:
	    Utils.UnitTools.ApplyFormation(scenario, echelon)