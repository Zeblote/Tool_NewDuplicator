// * ######################################################################
// *
// *    New Duplicator - Scripts - Server 
// *    Functions
// *
// *    -------------------------------------------------------------------
// *    General support functions that don't really fit in a specific file
// *
// * ######################################################################

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

//Get the closest paint color to an rgb value
function ndGetClosestColorID(%rgb)
{
	//Set initial value
	%color = getColorI(getColorIdTable(0));

	%best = 0;
	%bestDiff = vectorLen(vectorSub(%rgb, %color));

	if(getWord(%color, 3) != 255)
		%bestDiff += 1000;

	for(%i = 1; %i < 64; %i++)
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
	%color = getColorI(getColorIdTable(0));
	%alpha = getWord(%color, 3);

	%best = 0;
	%bestDiff = vectorLen(vectorSub(%rgb, %color));

	if((%alpha > 254 && %a < 254) || (%alpha < 254 && %a > 254))
		%bestDiff += 1000;
	else
		%bestDiff += mAbs(%alpha - %a) * 0.5;

	for(%i = 1; %i < 64; %i++)
	{
		%color = getColorI(getColorIdTable(%i));
		%alpha = getWord(%color, 3);

		%diff = vectorLen(vectorSub(%rgb, %color));

		if((%alpha > 254 && %a < 254) || (%alpha < 254 && %a > 254))
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
	if(%admin && !$Pref::Server::ND::AdminTrustRequired)
		return true;

	//Client can duplicate public bricks
	if(%group1.bl_id == 888888 && $Pref::Server::ND::SelectPublicBricks)
		return true;

	return false;
}

//Check whether a client has enough trust to modify a brick
function ndTrustCheckModify(%obj, %group2, %bl_id)
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

	return false;
}



//General stuff
///////////////////////////////////////////////////////////////////////////

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



//Binary compression
///////////////////////////////////////////////////////////////////////////

//Creates byte lookup table
function ndCreateByte241Table()
{
	$ND::Byte241Lookup = "";

	//This will map numbers 0-241 to chars 15-255, starting after \r
	for(%i = 15; %i < 256; %i++)
	{
		%char = collapseEscape("\\x" @
			getSubStr("0123456789abcdef", (%i & 0xf0) >> 4, 1) @
			getSubStr("0123456789abcdef", %i & 0x0f, 1));

		$ND::Byte241ToChar[%i - 15] = %char;
		$ND::Byte241Lookup = $ND::Byte241Lookup @ %char;
	}

	$ND::Byte241TableCreated = true;
}

//Packs number in single byte
function ndPack241_1(%num)
{
	return $ND::Byte241ToChar[%num];
}

//Packs number in two bytes
function ndPack241_2(%num)
{
	return $ND::Byte241ToChar[(%num / 241) | 0] @ $ND::Byte241ToChar[%num % 241];
}

//Packs number in three bytes
function ndPack241_3(%num)
{
	return
		$ND::Byte241ToChar[(((%num / 241) | 0) / 241) | 0] @
		$ND::Byte241ToChar[((%num / 241) | 0) % 241] @
		$ND::Byte241ToChar[%num % 241];
}

//Packs number in four bytes
function ndPack241_4(%num)
{
	return
		$ND::Byte241ToChar[(((((%num / 241) | 0) / 241) | 0) / 241) | 0] @
		$ND::Byte241ToChar[((((%num / 241) | 0) / 241) | 0) % 241] @
		$ND::Byte241ToChar[((%num / 241) | 0) % 241] @
		$ND::Byte241ToChar[%num % 241];
}

//Unpacks number from single byte
function ndUnpack241_1(%subStr)
{
	return strStr($ND::Byte241Lookup, %subStr);
}

//Unpacks number from two bytes
function ndUnpack241_2(%subStr)
{
	return
		strStr($ND::Byte241Lookup, getSubStr(%subStr, 0, 1)) * 241 +
		strStr($ND::Byte241Lookup, getSubStr(%subStr, 1, 1));
}

//Unpacks number from three bytes
function ndUnpack241_3(%subStr)
{
	return
		((strStr($ND::Byte241Lookup, getSubStr(%subStr, 0, 1)) * 58081) | 0) +
		  strStr($ND::Byte241Lookup, getSubStr(%subStr, 1, 1)) *   241       +
		  strStr($ND::Byte241Lookup, getSubStr(%subStr, 2, 1));
}

//Unpacks number from four bytes
function ndUnpack241_4(%subStr)
{
	return
		((strStr($ND::Byte241Lookup, getSubStr(%subStr, 0, 1)) * 13997521) | 0) +
		((strStr($ND::Byte241Lookup, getSubStr(%subStr, 1, 1)) *    58081) | 0) +
		  strStr($ND::Byte241Lookup, getSubStr(%subStr, 2, 1)) *      241       +
		  strStr($ND::Byte241Lookup, getSubStr(%subStr, 3, 1));
}
