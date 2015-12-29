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
