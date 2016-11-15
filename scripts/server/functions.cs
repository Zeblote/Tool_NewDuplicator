// This file should not exist. Fix later...
// -------------------------------------------------------------------

//Math functions
///////////////////////////////////////////////////////////////////////////

//Rotate vector around +Z in 90 degree steps
function ndRotateVector(%vector, %steps)
{
	switch(%steps % 4)
	{
		case 0: return %vector;
		case 1: return  getWord(%vector, 1) SPC -getWord(%vector, 0) SPC getWord(%vector, 2);
		case 2: return -getWord(%vector, 0) SPC -getWord(%vector, 1) SPC getWord(%vector, 2);
		case 3: return -getWord(%vector, 1) SPC  getWord(%vector, 0) SPC getWord(%vector, 2);
	}
}

//Rotate and mirror a direction
function ndTransformDirection(%dir, %steps, %mirrX, %mirrY, %mirrZ)
{
	if(%dir > 1)
	{
		if(%mirrX && %dir % 2 == 1
		|| %mirrY && %dir % 2 == 0)
			%dir += 2;

		%dir = (%dir + %steps - 2) % 4 + 2;
	}
	else if(%mirrZ)
		%dir = !%dir;

	return %dir;
}

//Get the closest paint color to an rgb value
function ndGetClosestColorID(%rgb)
{
	//Set initial value
	%best = 0;
	%bestDiff = 999999;

	for(%i = 0; %i < 64; %i++)
	{
		%color = getColorI(getColorIdTable(%i));

		%diff = vectorLen(vectorSub(%rgb, %color));

		if(getWord(%color, 3) != 255)
			%diff += 1000;

		if(%diff < %bestDiff)
		{
			%best = %i;
			%bestDiff = %diff;
		}
	}

	return %best;
}

//Get the closest paint color to an rgba value
function ndGetClosestColorID2(%rgba)
{
	%rgb = getWords(%rgba, 0, 2);
	%a = getWord(%rgba, 3);

	//Set initial value
	%best = 0;
	%bestDiff = 999999;

	for(%i = 0; %i < 64; %i++)
	{
		%color = getColorI(getColorIdTable(%i));
		%alpha = getWord(%color, 3);

		%diff = vectorLen(vectorSub(%rgb, %color));

		if((%alpha > 254) != (%a > 254))
			%diff += 1000;
		else
			%diff += mAbs(%alpha - %a) * 0.5;

		if(%diff < %bestDiff)
		{
			%best = %i;
			%bestDiff = %diff;
		}
	}

	return %best;
}

//Convert a paint color to a <color:xxxxxx> code
function ndGetPaintColorCode(%id)
{
	%rgb = getColorI(getColorIdTable(%id));
	%chars = "0123456789abcdef";

	%r = getWord(%rgb, 0);
	%g = getWord(%rgb, 1);
	%b = getWord(%rgb, 2);

	%r1 = getSubStr(%chars, (%r / 16) | 0, 1);
	%r2 = getSubStr(%chars,  %r % 16     , 1);

	%g1 = getSubStr(%chars, (%g / 16) | 0, 1);
	%g2 = getSubStr(%chars,  %g % 16     , 1);

	%b1 = getSubStr(%chars, (%b / 16) | 0, 1);
	%b2 = getSubStr(%chars,  %b % 16     , 1);

	return "<color:" @ %r1 @ %r2 @ %g1 @ %g2 @ %b1 @ %b2 @ ">";
}

//Get a plate world box from a raycast
function ndGetPlateBoxFromRayCast(%pos, %normal)
{
	//Get half size of world box for offset
	%halfSize = "0.25 0.25 0.1";

	//Point offset in correct direction based on normal
	%offX = getWord(%halfSize, 0) * mFloatLength(-getWord(%normal, 0), 0);
	%offY = getWord(%halfSize, 1) * mFloatLength(-getWord(%normal, 1), 0);
	%offZ = getWord(%halfSize, 2) * mFloatLength(-getWord(%normal, 2), 0);
	%offset = %offX SPC %offY SPC %offZ;

	//Get offset position
	%newPos = vectorAdd(%pos, %offset);

	//Get the plate box around the position
	%x1 = mFloor(getWord(%newPos, 0) * 2) / 2;
	%y1 = mFloor(getWord(%newPos, 1) * 2) / 2;
	%z1 = mFloor(getWord(%newPos, 2) * 5) / 5;

	%x2 = mCeil(getWord(%newPos, 0) * 2) / 2;
	%y2 = mCeil(getWord(%newPos, 1) * 2) / 2;
	%z2 = mCeil(getWord(%newPos, 2) * 5) / 5;

	return %x1 SPC %y1 SPC %z1 SPC %x2 SPC %y2 SPC %z2;
}



//Trust checks
///////////////////////////////////////////////////////////////////////////

//Send a message if a client doesn't have select trust to a brick
function ndTrustCheckMessage(%obj, %client)
{
	%group = %client.brickGroup.getId();
	%bl_id = %client.bl_id;
	%admin = %client.isAdmin;

	if(ndTrustCheckSelect(%obj, %group, %bl_id, %admin))
		return true;

	messageClient(%client, 'MsgError', "");
	commandToClient(%client, 'centerPrint', "<font:Verdana:20>\c6You don't have enough trust to do that!", 5);
	return false;
}

//Check whether a client has enough trust to select a brick
function ndTrustCheckSelect(%obj, %group2, %bl_id, %admin)
{
	%group1 = %obj.getGroup();

	//Client owns brick
	if(%group1 == %group2)
		return true;

	//Client owns stack
	if(%obj.stackBL_ID == %bl_id)
		return true;

	//Client has trust to the brick
	if(%group1.Trust[%bl_id] >= $Pref::Server::ND::TrustLimit)
		return true;

	//Client has trust to the stack of the brick
	if(%group2.Trust[%obj.stackBL_ID] >= $Pref::Server::ND::TrustLimit)
		return true;

	//Client is admin
	if(%admin && $Pref::Server::ND::AdminTrustBypass1)
		return true;

	//Client can duplicate public bricks
	if(%group1.bl_id == 888888 && $Pref::Server::ND::SelectPublicBricks)
		return true;

	return false;
}

//Check whether a client has enough trust to modify a brick
function ndTrustCheckModify(%obj, %group2, %bl_id, %admin)
{
	%group1 = %obj.getGroup();

	//Client owns brick
	if(%group1 == %group2)
		return true;

	//Client owns stack
	if(%obj.stackBL_ID == %bl_id)
		return true;

	//Client has trust to the brick
	if(%group1.Trust[%bl_id] >= 2)
		return true;

	//Client has trust to the stack of the brick
	if(%group2.Trust[%obj.stackBL_ID] >= 2)
		return true;

	//Client is admin
	if(%admin && $Pref::Server::ND::AdminTrustBypass2)
		return true;

	return false;
}

//Fast check whether a client has enough trust to plant on a brick
function ndFastTrustCheck(%brick, %bl_id, %brickGroup)
{
	%group = %brick.getGroup();

	if(%group == %brickGroup)
		return true;

	if(%group.Trust[%bl_id] > 0)
		return true;

	if(%group.bl_id == 888888)
		return true;

	return false;
}



//General stuff
///////////////////////////////////////////////////////////////////////////

//Setup list of spawned clients
function ndUpdateSpawnedClientList()
{
	$ND::NumSpawnedClients = 0;

	for(%i = 0; %i < ClientGroup.getCount(); %i++)
	{
		%cl = ClientGroup.getObject(%i);

		if(%cl.hasSpawnedOnce)
		{
			$ND::SpawnedClient[$ND::NumSpawnedClients] = %cl;
			$ND::NumSpawnedClients++;
		}
	}
}

//Applies mirror effect to a single ghost brick
function FxDtsBrick::ndMirrorGhost(%brick, %client, %axis)
{
	//Offset position
	%bPos = %brick.position;

	//Rotated local angle id
	%bAngle = %brick.angleID;

	//Apply mirror effects (ugh)
	%datablock = %brick.getDatablock();

	if(%axis == 0)
	{
		//Handle symmetries
		switch($ND::Symmetry[%datablock])
		{
			//Asymmetric
			case 0:
				if(%db = $ND::SymmetryXDatablock[%datablock])
				{
					%datablock = %db;
					%bAngle = (%bAngle + $ND::SymmetryXOffset[%datablock]) % 4;

					//Pair is made on X, so apply mirror logic for X afterwards
					if(%bAngle % 2 == 1)
						%bAngle = (%bAngle + 2) % 4;
				}
				else
				{
					messageClient(%client, '', "\c6Sorry, your ghost brick is asymmetric and cannot be mirrored.");
					return;
				}

			//Do nothing for fully symmetric

			//X symmetric - rotate 180 degrees if brick is angled 90 or 270 degrees
			case 2:
				if(%bAngle % 2 == 1)
					%bAngle = (%bAngle + 2) % 4;

			//Y symmetric - rotate 180 degrees if brick is angled 0 or 180 degrees
			case 3:
				if(%bAngle % 2 == 0)
					%bAngle = (%bAngle + 2) % 4;

			//X+Y symmetric - rotate 90 degrees
			case 4:
				if(%bAngle % 2 == 0)
					%bAngle = (%bAngle + 1) % 4;
				else
					%bAngle = (%bAngle + 3) % 4;

			//X-Y symmetric - rotate -90 degrees
			case 5:
				if(%bAngle % 2 == 0)
					%bAngle = (%bAngle + 3) % 4;
				else
					%bAngle = (%bAngle + 1) % 4;
		}
	}
	else if(%axis == 1)
	{
		//Handle symmetries
		switch($ND::Symmetry[%datablock])
		{
			//Asymmetric
			case 0:
				if(%db = $ND::SymmetryXDatablock[%datablock])
				{
					%datablock = %db;
					%bAngle = (%bAngle + $ND::SymmetryXOffset[%datablock]) % 4;

					//Pair is made on X, so apply mirror logic for X afterwards
					if(%bAngle % 2 == 0)
						%bAngle = (%bAngle + 2) % 4;
				}
				else
				{
					messageClient(%client, '', "\c6Sorry, your ghost brick is asymmetric and cannot be mirrored.");
					return;
				}

			//Do nothing for fully symmetric

			//X symmetric - rotate 180 degrees if brick is angled 90 or 270 degrees
			case 2:
				if(%bAngle % 2 == 0)
					%bAngle = (%bAngle + 2) % 4;

			//Y symmetric - rotate 180 degrees if brick is angled 0 or 180 degrees
			case 3:
				if(%bAngle % 2 == 1)
					%bAngle = (%bAngle + 2) % 4;

			//X+Y symmetric - rotate 90 degrees
			case 4:
				if(%bAngle % 2 == 1)
					%bAngle = (%bAngle + 1) % 4;
				else
					%bAngle = (%bAngle + 3) % 4;

			//X-Y symmetric - rotate -90 degrees
			case 5:
				if(%bAngle % 2 == 1)
					%bAngle = (%bAngle + 3) % 4;
				else
					%bAngle = (%bAngle + 1) % 4;
		}
	}
	else
	{
		//Change datablock if asymmetric
		if(!$ND::SymmetryZ[%datablock])
		{
			if(%db = $ND::SymmetryZDatablock[%datablock])
			{
				%datablock = %db;
				%bAngle = (%bAngle + $ND::SymmetryZOffset[%datablock]) % 4;
			}
			else
			{
				messageClient(%client, '', "\c6Sorry, your ghost brick is not vertically symmetric and cannot be mirrored.");
				return;
			}
		}
	}

	//Apply datablock
	if(%brick.getDatablock() != %datablock)
		%brick.setDatablock(%datablock);

	switch(%bAngle)
	{
		case 0: %bRot = "1 0 0 0";
		case 1: %bRot = "0 0 1 1.5708";
		case 2: %bRot = "0 0 1 3.14150";
		case 3: %bRot = "0 0 -1 1.5708";
	}

	//Apply transform
	%brick.setTransform(%bPos SPC %bRot);
}



//Supercut helpers
///////////////////////////////////////////////////////////////////////////

//Creates simple brick lookup table
function ndCreateSimpleBrickTable()
{
	deleteVariables("$ND::SimpleBrick*");
	%max = getDatablockGroupSize();

	%file = new FileObject();
	%sorter = new GuiTextListCtrl();

	for(%i = 0; %i < %max; %i++)
	{
		%db = getDatablock(%i);

		if(%db.getClassName() $= "FxDtsBrickData")
		{
			//Skip unsuitable bricks
			if(%db.isWaterBrick || %db.hasPrint || %db.isSlyrBrick
			|| %db.uiName $= "" || %db.ndDontUseForFill)
				continue;

			%file.openForRead(%db.brickFile);
			%file.readLine();

			//We only want simple bricks here
			if(%file.readLine() $= "BRICK")
			{
				//Skip brick sizes that we already have
				if(!$ND::SimpleBrickBlock[%db.brickSizeX, %db.brickSizeY, %db.BrickSizeZ])
					%sorter.addRow(%db, %db.getVolume());

				$ND::SimpleBrickBlock[%db.brickSizeX, %db.brickSizeY, %db.BrickSizeZ] = true;
			}

			%file.close();
		}
	}

	%file.delete();

	//Sort the bricks by volume
	%sorter.sortNumerical(0, 1);

	//Copy sorted bricks to global variable array
	$ND::SimpleBrickCount = %sorter.rowCount();
	for(%i = 0; %i < $ND::SimpleBrickCount; %i++)
	{
		%db = %sorter.getRowId(%i);
		%volume = %sorter.getRowText(%i);

		$ND::SimpleBrick[%i] = %db;
		$ND::SimpleBrickVolume[%i] = %volume;

		//Ensure X < Y in lookup table
		if(%db.brickSizeX <= %db.brickSizeY)
		{
			$ND::SimpleBrickSizeX[%i] = %db.brickSizeX;
			$ND::SimpleBrickSizeY[%i] = %db.brickSizeY;
			$ND::SimpleBrickRotated[%i] = false;
		}
		else
		{
			$ND::SimpleBrickSizeX[%i] = %db.brickSizeY;
			$ND::SimpleBrickSizeY[%i] = %db.brickSizeX;
			$ND::SimpleBrickRotated[%i] = true;
		}

		$ND::SimpleBrickSizeZ[%i] = %db.brickSizeZ;
	}

	%sorter.delete();
	$ND::SimpleBrickTableCreated = true;
}

//Find the largest (volume) brick that fits inside the area
function ndGetLargestBrickId(%x, %y, %z)
{
	if(!$ND::SimpleBrickTableCreated)
		ndCreateSimpleBrickTable();

	%maxVolume = %x * %y * %z;
	%start = $ND::SimpleBrickCount - 1;

	if($ND::SimpleBrickVolume[%start] > %maxVolume)
	{
		//Use binary search to find the largest volume that
		//is smaller or equal to the volume of the area
		%bound1 = 0;
		%bound2 = %start;

		while(%bound1 < %bound2)
		{
			%i = mCeil((%bound1 + %bound2) / 2);
			%volume = $ND::SimpleBrickVolume[%i];

			if(%volume > %maxVolume)
			{
				%bound2 = %i - 1;
				continue;
			}

			if(%volume <= %maxVolume)
			{
				%bound1 = %i + 1;
				continue;
			}
		}

		%start = %bound2;
	}

	%bestIndex = -1;

	//Go down the list until a brick fits on all 3 axis
	for(%i = %start; %i >= 0; %i--)
	{
		if($ND::SimpleBrickSizeX[%i] <= %x
		&& $ND::SimpleBrickSizeY[%i] <= %y
		&& $ND::SimpleBrickSizeZ[%i] <= %z)
		{
			return %i;
		}
	}

	return -1;
}

//Fill an area with bricks
function ndFillAreaWithBricks(%pos1, %pos2)
{
	%pos1_x = getWord(%pos1, 0);
	%pos1_y = getWord(%pos1, 1);
	%pos1_z = getWord(%pos1, 2);

	%pos2_x = getWord(%pos2, 0);
	%pos2_y = getWord(%pos2, 1);
	%pos2_z = getWord(%pos2, 2);

	%size_x = %pos2_x - %pos1_x;
	%size_y = %pos2_y - %pos1_y;
	%size_z = %pos2_z - %pos1_z;

	if(%size_x < 0.05 || %size_y < 0.05 || %size_z < 0.05)
		return;

	if(%size_x > %size_y)
	{
		%tmp = %size_y;
		%size_y = %size_x;
		%size_x = %tmp;
		%rotated = true;
	}

	%brickId = ndGetLargestBrickId(%size_x * 2 + 0.05, %size_y * 2 + 0.05, %size_z * 5 + 0.02);

	if(!%rotated)
	{
		%pos3_x = %pos1_x + $ND::SimpleBrickSizeX[%brickId] / 2;
		%pos3_y = %pos1_y + $ND::SimpleBrickSizeY[%brickId] / 2;
	}
	else
	{
		%pos3_x = %pos1_x + $ND::SimpleBrickSizeY[%brickId] / 2;
		%pos3_y = %pos1_y + $ND::SimpleBrickSizeX[%brickId] / 2;
	}

	%pos3_z = %pos1_z + $ND::SimpleBrickSizeZ[%brickId] / 5;
	%plantPos = (%pos1_x + %pos3_x) / 2 SPC (%pos1_y + %pos3_y) / 2 SPC (%pos1_z + %pos3_z) / 2;

	if(!isObject($ND::SimpleBrick[%brickId]))
		return;

	%brick = new FxDTSBrick()
	{
		datablock = $ND::SimpleBrick[%brickId];
		isPlanted = true;
		client = $ND::FillBrickClient;

		position = %plantPos;
		rotation = (%rotated ^ $ND::SimpleBrickRotated[%brickId]) ? "0 0 1 90.0002" : "1 0 0 0";
		angleID = %rotated;

		colorID = $ND::FillBrickColorID;
		colorFxID = $ND::FillBrickColorFxID;
		shapeFxID = $ND::FillBrickShapeFxID;

		printID = 0;
	};

	//This will call ::onLoadPlant instead of ::onPlant
	%prev1 = $Server_LoadFileObj;
	%prev2 = $LastLoadedBrick;
	$Server_LoadFileObj = %brick;
	$LastLoadedBrick = %brick;

	//Add to brickgroup
	$ND::FillBrickGroup.add(%brick);

	//Attempt plant
	%error = %brick.plant();

	//Restore variable
	$Server_LoadFileObj = %prev1;
	$LastLoadedBrick = %prev2;

	if(!%error || %error == 2)
	{
		//Set trusted
		if(%brick.getNumDownBricks())
			%brick.stackBL_ID = %brick.getDownBrick(0).stackBL_ID;
		else if(%brick.getNumUpBricks())
			%brick.stackBL_ID = %brick.getUpBrick(0).stackBL_ID;
		else
			%brick.stackBL_ID = $ND::FillBrickBL_ID;

		%brick.trustCheckFinished();

		%brick.setRendering($ND::FillBrickRendering);
		%brick.setColliding($ND::FillBrickColliding);
		%brick.setRayCasting($ND::FillBrickRayCasting);

		//Instantly ghost the brick to all spawned clients (wow hacks)
		for(%j = 0; %j < $ND::NumSpawnedClients; %j++)
		{
			%cl = $ND::SpawnedClient[%j];
			%brick.scopeToClient(%cl);
			%brick.clearScopeToClient(%cl);
		}

		$ND::FillBrickCount++;
	}
	else
		%brick.delete();

	if((%pos3_x + 0.05) < %pos2_x)
		ndFillAreaWithBricks(%pos3_x SPC %pos1_y SPC %pos1_z, %pos2_x SPC %pos2_y SPC %pos2_z);

	if((%pos3_y + 0.05) < %pos2_y)
		ndFillAreaWithBricks(%pos1_x SPC %pos3_y SPC %pos1_z, %pos3_x SPC %pos2_y SPC %pos2_z);

	if((%pos3_z + 0.02) < %pos2_z)
		ndFillAreaWithBricks(%pos1_x SPC %pos1_y SPC %pos3_z, %pos3_x SPC %pos3_y SPC %pos2_z);

}

//Client finished supercut, now fill bricks
function GameConnection::doFillBricks(%this)
{
	//Set variables for the fill brick function
	$ND::FillBrickGroup = %this.brickGroup;
	$ND::FillBrickClient = %this;
	$ND::FillBrickBL_ID = %this.bl_id;

	$ND::FillBrickColorID = %this.currentColor;
	$ND::FillBrickColorFxID = 0;
	$ND::FillBrickShapeFxID = 0;

	$ND::FillBrickRendering = true;
	$ND::FillBrickColliding = true;
	$ND::FillBrickRayCasting = true;

	%box = %this.ndSelectionBox.getWorldBox();
	$ND::FillBrickCount = 0;
	ndUpdateSpawnedClientList();
	ndFillAreaWithBricks(getWords(%box, 0, 2), getWords(%box, 3, 5));

	%s = ($ND::FillBrickCount == 1 ? "" : "s");
	messageClient(%this, '', "\c6Filled in \c3" @ $ND::FillBrickCount @ "\c6 brick" @ %s);
}
