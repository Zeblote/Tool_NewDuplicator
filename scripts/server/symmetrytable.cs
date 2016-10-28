// Analyzes brick geometry to detect common symmetry planes.
// -------------------------------------------------------------------

//Begin generating the symmetry table
function ndCreateSymmetryTable()
{
	if($ND::SymmetryTableCreating)
		return;

	//Tell everyone what is happening
	messageAll('', "\c6(\c3New Duplicator\c6) \c6Creating brick symmetry table...");

	//Make sure we have the uiname table for manual symmetry
	if(!$UINameTableCreated)
		createUINameTable();

	//Delete previous data
	deleteVariables("$ND::Symmetry*");

	$ND::SymmetryTableCreated = false;
	$ND::SymmetryTableCreating = true;
	$ND::SymmetryTableStarted = getRealTime();

	$NDT::SimpleCount = 0;
	$NDT::MeshCount = 0;

	$NDT::AsymXCountTotal = 0;
	$NDT::AsymZCountTotal = 0;

	echo("ND: Start building brick symmetry table...");
	echo("==========================================================================");

	ndTickCreateSymmetryTable(0, getDatablockGroupSize());
}

//Process symmetry for 6 datablocks each tick
function ndTickCreateSymmetryTable(%lastIndex, %max)
{
	%processed = 0;
	%limit = $Server::Dedicated ? 400 : 200;

	for(%i = %lastIndex; %i < %max; %i++)
	{
		%db = getDatablock(%i);

		if(%db.getClassName() $= "FxDtsBrickData")
		{
			%processed += ndTestBrickSymmetry(%db);

			if(%processed > %limit)
			{
				schedule(30, 0, ndTickCreateSymmetryTable, %i + 1, %max);
				return;
			}
		}
	}

	%simple = $NDT::SimpleCount;
	%mesh = $NDT::MeshCount;

	%asymx = $NDT::AsymXCountTotal;
	%asymz = $NDT::AsymZCountTotal;

	echo("==========================================================================");
	echo("ND: Finished basic symmetry tests: " @ %simple @ " simple, " @ %mesh @ " with mesh, " @ %asymx @ " asymmetric, " @ %asymz @ " z-asymmetric");

	ndFindSymmetricPairs();
}

//Attempt to find symmetric pairs between asymmetric bricks
function ndFindSymmetricPairs()
{
	echo("ND: Starting X symmetric pair search...");
	echo("==========================================================================");

	for(%i = 0; %i < $NDT::AsymXCountTotal; %i++)
	{
		%index = $NDT::AsymXBrick[%i];

		if(!$NDT::SkipAsymX[%index])
			ndFindSymmetricPairX(%index);
	}

	echo("==========================================================================");
	echo("ND: Finished finding symmetric pairs");
	echo("ND: Starting Z symmetric pair search...");
	echo("==========================================================================");

	for(%i = 0; %i < $NDT::AsymZCountTotal; %i++)
	{
		%index = $NDT::AsymZBrick[%i];

		if(!$NDT::SkipAsymZ[%index])
			ndFindSymmetricPairZ(%index);
	}

	//Delete temporary arrays
	deleteVariables("$NDT::*");

	echo("==========================================================================");
	echo("ND: Finished finding Z symmetric pairs");
	echo("ND: Symmetry table complete in " @ (getRealTime() - $ND::SymmetryTableStarted) / 1000 @ " seconds");

	$ND::SymmetryTableCreated = true;
	$ND::SymmetryTableCreating = false;

	//We're done!
	%seconds = mFloatLength((getRealTime() - $ND::SymmetryTableStarted) / 1000, 0);
	messageAll('', "\c6(\c3New Duplicator\c6) \c6Created brick symmetry table in " @ %seconds @ " seconds.");
}

//Test symmetry of a single blb file
function ndTestBrickSymmetry(%datablock)
{
	//Open blb file
	%file = new FileObject();
	%file.openForRead(%datablock.brickFile);

	//Skip brick size - irrelevant
	%file.readLine();

	//Simple bricks are always fully symmetric
	if(%file.readLine() $= "BRICK")
	{
		$NDT::SimpleCount++;

		$ND::Symmetry[%datablock] = 1;
		$ND::SymmetryZ[%datablock] = true;

		%file.close();
		%file.delete();
		return 2;
	}

	//Not simple, get mesh data index in temp arrays
	%dbi = $NDT::MeshCount;
	$NDT::MeshCount++;

	//Load mesh from blb file
	%faces = 0;
	%points = 0;

	while(!%file.isEOF())
	{
		//Find start of face
		%line = %file.readLine();

		if(getSubStr(%line, 0, 4) $= "TEX:")
		{
			%tex = trim(getSubStr(%line, 4, strLen(%line)));

			//Top and bottom faces have different topology, skip
			if(%tex $= "TOP" || %tex $= "BOTTOMLOOP" || %tex $= "BOTTOMEDGE")
				continue;

			//Add face
			$NDT::FaceTexId[%dbi, %faces] = (%tex $= "SIDE" ? 0 : (%tex $= "RAMP" ? 1 : 2));

			//Skip useless lines
			while(trim(%file.readLine()) !$= "POSITION:") {}

			//Add the 4 points
			for(%i = 0; %i < 4; %i++)
			{
				//Read next line
				%line = %file.readLine();

				//Skip useless blank lines
				while(!strLen(%line))
					%line = %file.readLine();

				//Remove formatting from point
				%pos = vectorAdd(%line, "0 0 0");

				//Round down two digits to fix float errors
				%pos = mFloatLength(getWord(%pos, 0), 3) * 1.0
				   SPC mFloatLength(getWord(%pos, 1), 3) * 1.0
				   SPC mFloatLength(getWord(%pos, 2), 3) * 1.0;

				//Get index of this point
				if(!%ptIndex = $NDT::PtAtPosition[%dbi, %pos])
				{
					//Points array is 1-indexed so we can quickly test !PtAtPosition[...]
					%points++;
					%ptIndex = %points;

					//Add new point to array
					$NDT::PtPosition[%dbi, %points] = %pos;
					$NDT::PtAtPosition[%dbi, %pos] = %points;
				}

				//Add face to point
				if(!$NDT::PtInFace[%dbi, %faces, %ptIndex])
				{
					//Increase first then subtract 1 to get 0 the first time
					%fIndex = $NDT::FacesAtPt[%dbi, %ptIndex]++ - 1;
					$NDT::FaceAtPt[%dbi, %ptIndex, %fIndex] = %faces;
				}

				//Add point to face
				$NDT::FacePt[%dbi, %faces, %i] = %ptIndex;
				$NDT::PtInFace[%dbi, %faces, %ptIndex] = true;
			}

			//Added face
			%faces++;
		}
	}

	$NDT::FaceCount[%dbi] = %faces;
	$NDT::Datablock[%dbi] = %datablock;

	%file.close();
	%file.delete();

	//Possible symmetries:
	// 0: asymmetric
	// 1: x & y
	// 2: x
	// 3: y
	// 4: x+y
	// 5: x-y

	//We will test in the following order:
	// X
	// Y
	// X+Y
	// X-Y
	// Z

	//Check manual symmetry first
	%sym = $ND::ManualSymmetry[%datablock.uiname];

	if(%sym !$= "")
	{
		if(!%sym)
		{
			//Try to find the other brick
			%otherdb = $UINameTable[$ND::ManualSymmetryDB[%datablock.uiname]];
			%offset = $ND::ManualSymmetryOffset[%datablock.uiname];

			//...
			if(!isObject(%otherdb))
			{
				%otherdb = "";
				%offset = 0;
				echo("ND: " @ %datablock.uiname @ " has manual symmetry but the paired brick does not exist");
			}

			$ND::SymmetryXDatablock[%datablock] = %otherdb;
			$ND::SymmetryXOffset[%datablock] = %offset;
		}

		%manualSym = true;
	}
	else
	{
		%failX = ndTestSelfSymmetry(%dbi, 0);
		%failY = ndTestSelfSymmetry(%dbi, 1);

		//Diagonals are only needed if the brick isn't symmetric to the axis
		if(%failX && %failY)
			%failXY = ndTestSelfSymmetry(%dbi, 3);

		//One diagonal is enough, only test second if first one fails
		if(%failXY)
			%failYX = ndTestSelfSymmetry(%dbi, 4);

		//X, Y symmetry
		if(!%failX && !%failY)
			%sym = 1;
		else if(!%failX)
			%sym = 2;
		else if(!%failY)
			%sym = 3;
		else if(!%failXY)
			%sym = 4;
		else if(!%failYX)
			%sym = 5;
		else
			%sym = 0;
	}

	//Check manual symmetry first
	%symZ = $ND::ManualSymmetryZ[%datablock.uiname];

	//Z symmetry
	if(%symZ !$= "")
	{
		if(!%symZ)
		{
			//Try to find the other brick
			%otherdb = $UINameTable[$ND::ManualSymmetryZDB[%datablock.uiname]];
			%offset = $ND::ManualSymmetryZOffset[%datablock.uiname];

			//...
			if(!isObject(%otherdb))
			{
				%otherdb = "";
				%offset = 0;
				echo("ND: " @ %datablock.uiname @ " has manual Z symmetry but the paired brick does not exist");
			}

			$ND::SymmetryZDatablock[%datablock] = %otherdb;
			$ND::SymmetryZOffset[%datablock] = %offset;
		}

		%manualZSym = true;
	}
	else
		%symZ = !ndTestSelfSymmetry(%dbi, 2);

	if(!%manualSym && !%sym)
	{
		//Add to lookup table of X-asymmetric bricks of this type
		%bIndex = $NDT::AsymXCount[%faces, %symZ]++;
		$NDT::AsymXBrick[%faces, %symZ, %bIndex] = %dbi;

		//Add to list of asymmetric bricks
		$NDT::AsymXBrick[$NDT::AsymXCountTotal] = %dbi;
		$NDT::AsymXCountTotal++;
	}

	if(!%manualZSym && !%symZ)
	{
		//Add to lookup table of Z-asymmetric bricks of this type
		%bIndex = $NDT::AsymZCount[%faces, %sym]++;
		$NDT::AsymZBrick[%faces, %sym, %bIndex] = %dbi;

		//Add to list of Z-asymmetric bricks
		$NDT::AsymZBrick[$NDT::AsymZCountTotal] = %dbi;
		$NDT::AsymZCountTotal++;
	}

	//Save symmetries
	$ND::Symmetry[%datablock] = %sym;
	$ND::SymmetryZ[%datablock] = %symZ;

	//Return processed faces
	return %faces;
}

//Find symmetric pair between two bricks on X axis
function ndFindSymmetricPairX(%dbi)
{
	if($NDT::SkipAsymX[%dbi])
		return;

	%datablock = $NDT::Datablock[%dbi];

	%zsym = $ND::SymmetryZ[%datablock];
	%faces = $NDT::FaceCount[%dbi];
	%count = $NDT::AsymXCount[%faces, %zsym];

	//Only potential match is the brick itself - fail
	if(%count == 1)
	{
		echo("ND: No X match for " @ %datablock.getName() @ " (" @ %datablock.category @ "/" @ %datablock.subCategory @ "/" @ %datablock.uiname @ ")");
		return;
	}

	%off = -1;
	$NDT::SkipAsymX[%dbi] = true;

	for(%i = 1; %i <= %count; %i++)
	{
		%other = $NDT::AsymXBrick[%faces, %zsym, %i];

		//Don't compare with bricks that already have a pair
		if($NDT::SkipAsymX[%other])
			continue;

		//Test all 4 possible rotations
		//Not using loop due to lack of goto command
		if(!ndTestPairSymmetry(%dbi, %other, true, 0))
		{
			%off = 0;
			break;
		}

		if(!ndTestPairSymmetry(%dbi, %other, true, 1))
		{
			%off = 1;
			break;
		}

		if(!ndTestPairSymmetry(%dbi, %other, true, 2))
		{
			%off = 2;
			break;
		}

		if(!ndTestPairSymmetry(%dbi, %other, true, 3))
		{
			%off = 3;
			break;
		}
	}

	if(%off != -1)
	{
		%otherdb = $NDT::Datablock[%other];

		//Save symmetry
		$ND::SymmetryXDatablock[%datablock] = %otherdb;
		$ND::SymmetryXOffset[%datablock] = %off;

		$ND::SymmetryXDatablock[%otherdb] = %datablock;
		$ND::SymmetryXOffset[%otherdb] = -%off;

		//No need to process the other brick again
		$NDT::SkipAsymX[%other] = true;
	}
	else
		echo("ND: No X match for " @ %datablock.getName() @ " (" @ %datablock.category @ "/" @ %datablock.subCategory @ "/" @ %datablock.uiname @ ")");
}

//Find symmetric pair between two bricks on Z axis
function ndFindSymmetricPairZ(%dbi)
{
	if($NDT::SkipAsymZ[%dbi])
		return;

	%datablock = $NDT::Datablock[%dbi];

	%sym = $ND::Symmetry[%datablock];
	%faces = $NDT::FaceCount[%dbi];
	%count = $NDT::AsymZCount[%faces, %sym];

	//Only potential match is the brick itself - fail
	if(%count == 1)
	{
		echo("ND: No Z match for " @ %datablock.getName() @ " (" @ %datablock.category @ "/" @ %datablock.subCategory @ "/" @ %datablock.uiname @ ")");
		return;
	}

	%off = -1;

	for(%i = 1; %i <= %count; %i++)
	{
		%other = $NDT::AsymZBrick[%faces, %sym, %i];

		//Don't compare with bricks that already have a pair
		if($NDT::SkipAsymZ[%other])
			continue;

		//Test all 4 possible rotations
		//Not using loop due to lack of goto command
		if(!ndTestPairSymmetry(%dbi, %other, false, 0))
		{
			%off = 0;
			break;
		}

		if(!ndTestPairSymmetry(%dbi, %other, false, 1))
		{
			%off = 1;
			break;
		}

		if(!ndTestPairSymmetry(%dbi, %other, false, 2))
		{
			%off = 2;
			break;
		}

		if(!ndTestPairSymmetry(%dbi, %other, false, 3))
		{
			%off = 3;
			break;
		}
	}

	//It's possible for a brick to match itself rotated
	//here, so only mark it after the search
	$NDT::SkipAsymZ[%dbi] = true;

	if(%off != -1)
	{
		%otherdb = $NDT::Datablock[%other];

		//Save symmetry
		$ND::SymmetryZDatablock[%datablock] = %otherdb;
		$ND::SymmetryZOffset[%datablock] = %off;

		$ND::SymmetryZDatablock[%otherdb] = %datablock;
		$ND::SymmetryZOffset[%otherdb] = -%off;

		//No need to process the other brick again
		$NDT::SkipAsymZ[%other] = true;
	}
	else
		echo("ND: No Z match for " @ %datablock.getName() @ " (" @ %datablock.category @ "/" @ %datablock.subCategory @ "/" @ %datablock.uiname @ ")");
}

//Test a mesh for a single symmetry plane in itself
function ndTestSelfSymmetry(%dbi, %plane)
{
	%fail = false;
	%faces = $NDT::FaceCount[%dbi];

	for(%i = 0; %i < %faces; %i++)
	{
		//If this face was already used by another mirror, skip
		if(%skipFace[%i])
			continue;

		//Attempt to find the mirrored points
		for(%j = 0; %j < 4; %j++)
		{
			%pt = $NDT::FacePt[%dbi, %i, %j];

			//Do we already know the mirrored one?
			if(%mirrPt[%pt])
			{
				%mirr[%j] = %mirrPt[%pt];
				continue;
			}

			//Get position of point
			%v = $NDT::PtPosition[%dbi, %pt];

			//Get point at mirrored position based on plane
			switch$(%plane)
			{
				//Flip X
				case 0:  %mirr = $NDT::PtAtPosition[%dbi, -firstWord(%v) SPC restWords(%v)];
				//Flip Y
				case 1:  %mirr = $NDT::PtAtPosition[%dbi, getWord(%v, 0) SPC -getWord(%v, 1) SPC getWord(%v, 2)];
				//Flip Z
				case 2:  %mirr = $NDT::PtAtPosition[%dbi, getWords(%v, 0, 1) SPC -getWord(%v, 2)];
				//Mirror along X+Y
				case 3:  %mirr = $NDT::PtAtPosition[%dbi, -getWord(%v, 1) SPC -getWord(%v, 0) SPC getWord(%v, 2)];
				//Mirror along X-Y
				default: %mirr = $NDT::PtAtPosition[%dbi, getWord(%v, 1) SPC getWord(%v, 0) SPC getWord(%v, 2)];
			}

			if(%mirr)
			{
				%mirrPt[%pt] = %mirr;
				%mirrPt[%mirr] = %pt;

				%mirr[%j] = %mirr;
			}
			else
			{
				%fail = true;
				break;
			}
		}

		if(%fail)
			break;

		//Test whether the points have a common face
		%fail = true;
		%count = $NDT::FacesAtPt[%dbi, %mirr0];

		for(%j = 0; %j < %count; %j++)
		{
			%potentialFace = $NDT::FaceAtPt[%dbi, %mirr0, %j];

			//Mirrored face must have the same texture id
			if($NDT::FaceTexId[%dbi, %i] != $NDT::FaceTexId[%dbi, %potentialFace])
				continue;

			//Check whether remaining points are in the face
			if(!$NDT::PtInFace[%dbi, %potentialFace, %mirr1])
				continue;

			if(!$NDT::PtInFace[%dbi, %potentialFace, %mirr2])
				continue;

			if(!$NDT::PtInFace[%dbi, %potentialFace, %mirr3])
				continue;

			//We found a matching face!
			%skipFace[%potentialFace] = true;
			%fail = false;
			break;
		}

		if(%fail)
			break;
	}

	return %fail;
}

//Test X or Z symmetry between two meshes with rotation offset
function ndTestPairSymmetry(%dbi, %other, %plane, %rotation)
{
	%fail = false;
	%faces = $NDT::FaceCount[%dbi];

	for(%i = 0; %i < %faces; %i++)
	{
		//Attempt to find the mirrored points
		for(%j = 0; %j < 4; %j++)
		{
			%pt = $NDT::FacePt[%dbi, %i, %j];

			//Do we already know the mirrored one?
			if(%mirrPt[%pt])
			{
				%mirr[%j] = %mirrPt[%pt];
				continue;
			}

			//Get position of point
			%v = $NDT::PtPosition[%dbi, %pt];

			//true = X, false = Z
			if(%plane)
			{
				//Get point at mirrored position based on rotation
				switch(%rotation)
				{
					//Flip X
					case 0:  %mirr = $NDT::PtAtPosition[%other, -firstWord(%v) SPC restWords(%v)];
					//Flip X, rotate 90
					case 1:  %mirr = $NDT::PtAtPosition[%other, getWord(%v, 1) SPC getWord(%v, 0) SPC getWord(%v, 2)];
					//Flip X, rotate 180
					case 2:  %mirr = $NDT::PtAtPosition[%other, getWord(%v, 0) SPC -getWord(%v, 1) SPC getWord(%v, 2)];
					//Flip X, rotate 270
					default: %mirr = $NDT::PtAtPosition[%other, -getWord(%v, 1) SPC -getWord(%v, 0) SPC getWord(%v, 2)];
				}
			}
			else
			{
				//Get point at mirrored position based on rotation
				switch(%rotation)
				{
					//Flip Z
					case 0:  %mirr = $NDT::PtAtPosition[%other, getWord(%v, 0) SPC getWord(%v, 1) SPC -getWord(%v, 2)];
					//Flip Z, rotate 90
					case 1:  %mirr = $NDT::PtAtPosition[%other, getWord(%v, 1) SPC -getWord(%v, 0) SPC -getWord(%v, 2)];
					//Flip Z, rotate 180
					case 2:  %mirr = $NDT::PtAtPosition[%other, -getWord(%v, 0) SPC -getWord(%v, 1) SPC -getWord(%v, 2)];
					//Flip Z, rotate 270
					default: %mirr = $NDT::PtAtPosition[%other, -getWord(%v, 1) SPC getWord(%v, 0) SPC -getWord(%v, 2)];
				}
			}

			if(%mirr)
			{
				%mirrPt[%pt] = %mirr;
				%mirr[%j] = %mirr;
			}
			else
			{
				%fail = true;
				break;
			}
		}

		if(%fail)
			break;

		//Test whether the points have a common face
		%fail = true;
		%count = $NDT::FacesAtPt[%other, %mirr0];

		for(%j = 0; %j < %count; %j++)
		{
			%potentialFace = $NDT::FaceAtPt[%other, %mirr0, %j];

			//Mirrored face must have the same texture id
			if($NDT::FaceTexId[%dbi, %i] != $NDT::FaceTexId[%other, %potentialFace])
				continue;

			//Check whether remaining points are in the face
			if(!$NDT::PtInFace[%other, %potentialFace, %mirr1])
				continue;

			if(!$NDT::PtInFace[%other, %potentialFace, %mirr2])
				continue;

			if(!$NDT::PtInFace[%other, %potentialFace, %mirr3])
				continue;

			//We found a matching face!
			%fail = false;
			break;
		}

		if(%fail)
			break;
	}

	return %fail;
}
