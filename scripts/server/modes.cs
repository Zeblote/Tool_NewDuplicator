// * ######################################################################
// *
// *    New Duplicator - Scripts - Server
// *    Duplicator Modes
// *
// *    -------------------------------------------------------------------
// *    Handles registering and switching between duplicator modes
// *
// * ######################################################################

//Base class for all duplicator modes
///////////////////////////////////////////////////////////////////////////

function NewDuplicatorMode::onStartMode(%this, %client, %lastMode){}
function NewDuplicatorMode::onChangeMode(%this, %client, %nextMode){}
function NewDuplicatorMode::onKillMode(%this, %client){}

function NewDuplicatorMode::onSelectObject(%this, %client, %obj, %pos, %normal){}

function NewDuplicatorMode::onLight(%this, %client){}
function NewDuplicatorMode::onNextSeat(%this, %client){}
function NewDuplicatorMode::onPrevSeat(%this, %client){}
function NewDuplicatorMode::onShiftBrick(%this, %client, %x, %y, %z){}
function NewDuplicatorMode::onSuperShiftBrick(%this, %client, %x, %y, %z){}
function NewDuplicatorMode::onRotateBrick(%this, %client, %direction){}
function NewDuplicatorMode::onPlantBrick(%this, %client){}
function NewDuplicatorMode::onCancelBrick(%this, %client){}

function NewDuplicatorMode::onCopy(%this, %client)
{
	messageClient(%client, '', "\c6Copy can not be used in your current duplicator mode.");
}

function NewDuplicatorMode::onPaste(%this, %client)
{
	messageClient(%client, '', "\c6Paste can not be used in your current duplicator mode.");
}

function NewDuplicatorMode::onCut(%this, %client)
{
	messageClient(%client, '', "\c6Cut can not be used in your current duplicator mode.");
}

function NewDuplicatorMode::getBottomPrint(%this, %client){}



//Registering duplicator modes
///////////////////////////////////////////////////////////////////////////

//Possible mode indices
$NDM::Disabled            =  0;
$NDM::CubeSelect          =  1;
$NDM::CubeSelectProgress  =  2;
$NDM::CutProgress         =  3;
$NDM::FillColor           =  4;
$NDM::FillColorProgress   =  5;
$NDM::StackSelect         =  6;
$NDM::StackSelectProgress =  7;
$NDM::PlantCopy           =  8;
$NDM::PlantCopyProgress   =  9;
$NDM::WrenchProgress      = 10;
$NDM::SaveProgress        = 11;
$NDM::LoadProgress        = 12;

//Create all the pseudo-classes to handle callbacks
function ndRegisterDuplicatorModes()
{
	echo("ND: Registering duplicator modes");

	//Disabled duplicator mode (does nothing)
	ND_ServerGroup.add(
		new ScriptObject(NDM_Disabled)
		{
			class = "NewDuplicatorMode";
			index = $NDM::Disabled;
			image = "ND_Image";
			spin = false;

			allowSelecting = false;
			allowUnMount   = false;
		}
	);

	//Cube Select duplicator mode
	ND_ServerGroup.add(
		new ScriptObject(NDM_CubeSelect)
		{
			class = "NewDuplicatorMode";
			index = $NDM::CubeSelect;
			image = "ND_Image_Cube";
			spin = false;

			allowSelecting = true;
			allowUnMount   = false;
		}
	);

	//Cube Select Progress duplicator mode
	ND_ServerGroup.add(
		new ScriptObject(NDM_CubeSelectProgress)
		{
			class = "NewDuplicatorMode";
			index = $NDM::CubeSelectProgress;
			image = "ND_Image_Cube";
			spin = true;

			allowSelecting = false;
			allowUnMount   = false;
		}
	);

	//Cut Progress duplicator mode
	ND_ServerGroup.add(
		new ScriptObject(NDM_CutProgress)
		{
			class = "NewDuplicatorMode";
			index = $NDM::CutProgress;
			image = "any";
			spin = true;

			allowSelecting = false;
			allowUnMount   = false;
		}
	);

	//Fill Color duplicator mode
	ND_ServerGroup.add(
		new ScriptObject(NDM_FillColor)
		{
			class = "NewDuplicatorMode";
			index = $NDM::FillColor;
			image = "any";
			spin = false;

			allowSelecting = false;
			allowUnMount   = false;
		}
	);

	//Fill Color Progress duplicator mode
	ND_ServerGroup.add(
		new ScriptObject(NDM_FillColorProgress)
		{
			class = "NewDuplicatorMode";
			index = $NDM::FillColorProgress;
			image = "any";
			spin = true;

			allowSelecting = false;
			allowUnMount   = false;
		}
	);

	//Plant Copy duplicator mode
	ND_ServerGroup.add(
		new ScriptObject(NDM_PlantCopy)
		{
			class = "NewDuplicatorMode";
			index = $NDM::PlantCopy;
			image = "ND_Image_Blue";
			spin = false;

			allowSelecting = true;
			allowUnMount   = true;
		}
	);

	//Plant Copy Progress duplicator mode
	ND_ServerGroup.add(
		new ScriptObject(NDM_PlantCopyProgress)
		{
			class = "NewDuplicatorMode";
			index = $NDM::PlantCopyProgress;
			image = "ND_Image_Blue";
			spin = true;

			allowSelecting = false;
			allowUnMount   = true;
		}
	);

	//Stack Select duplicator mode
	ND_ServerGroup.add(
		new ScriptObject(NDM_StackSelect)
		{
			class = "NewDuplicatorMode";
			index = $NDM::StackSelect;
			image = "ND_Image";
			spin = false;

			allowSelecting = true;
			allowUnMount   = false;
		}
	);

	//Stack Select Progress duplicator mode
	ND_ServerGroup.add(
		new ScriptObject(NDM_StackSelectProgress)
		{
			class = "NewDuplicatorMode";
			index = $NDM::StackSelectProgress;
			image = "ND_Image";
			spin = true;

			allowSelecting = false;
			allowUnMount   = false;
		}
	);

	//Wrench Progress duplicator mode
	ND_ServerGroup.add(
		new ScriptObject(NDM_WrenchProgress)
		{
			class = "NewDuplicatorMode";
			index = $NDM::WrenchProgress;
			image = "any";
			spin = true;

			allowSelecting = false;
			allowUnMount   = false;
		}
	);

	//Save Progress duplicator mode
	ND_ServerGroup.add(
		new ScriptObject(NDM_SaveProgress)
		{
			class = "NewDuplicatorMode";
			index = $NDM::SaveProgress;
			image = "any";
			spin = true;

			allowSelecting = false;
			allowUnMount   = false;
		}
	);

	//Load Progress duplicator mode
	ND_ServerGroup.add(
		new ScriptObject(NDM_LoadProgress)
		{
			class = "NewDuplicatorMode";
			index = $NDM::LoadProgress;
			image = "any";
			spin = true;

			allowSelecting = false;
			allowUnMount   = false;
		}
	);

	//If clients already exist, reset their modes
	for(%i = 0; %i < ClientGroup.getCount(); %i++)
	{
		%cl = ClientGroup.getObject(%i);

		%cl.ndPivot      = true;
		%cl.ndLimited    = true;
		%cl.ndDirection  = true;
		%cl.ndForcePlant = false;

		%cl.ndImage          = ND_Image.getId();
		%cl.ndMode           = NDM_Disabled;
		%cl.ndModeIndex      = $NDM::Disabled;
		%cl.ndLastSelectMode = NDM_StackSelect;
	}
}



//Switching modes
///////////////////////////////////////////////////////////////////////////

//Change duplication mode
function GameConnection::ndSetMode(%this, %newMode)
{
	%oldMode = %this.ndMode;

	if(%oldMode.index == %newMode.index)
		return;

	%this.ndMode      = %newMode;
	%this.ndModeIndex = %newMode.index;

	%oldMode.onChangeMode(%this, %newMode.index);
	%newMode.onStartMode(%this, %oldMode.index);

	//Enable keybinds
	if(!%oldMode.index)
		commandToClient(%this, 'ndEnableKeybinds', true);

	//Change image
	if(%newMode.image !$= "any")
		%this.ndSetImage(nameToId(%newMode.image));

	//Start or stop spinning
	%this.player.setImageLoaded(0, !%newMode.spin);
}

//Kill duplication mode
function GameConnection::ndKillMode(%this)
{
	if(!%this.ndModeIndex)
		return;

	%this.ndMode.onKillMode(%this);

	%this.ndMode = NDM_Disabled;
	%this.ndModeIndex = $NDM::Disabled;

	%this.ndUpdateBottomPrint();

	//Disable keybinds
	commandToClient(%this, 'ndEnableKeybinds', false);
}



//Bottomprints
///////////////////////////////////////////////////////////////////////////

//Update the bottomprint
function GameConnection::ndUpdateBottomPrint(%this)
{
	if(%this.ndModeIndex)
		commandToClient(%this, 'bottomPrint', %this.ndMode.getBottomPrint(%this), 0, true);
	else
		commandToClient(%this, 'clearBottomPrint');
}

//Format bottomprint message with left and right justified text
function ndFormatMessage(%title, %l0, %r0, %l1, %r1, %l2, %r2)
{
	%message = "<font:Arial:22>";

	//Last used alignment, false = left | true = right
	%align = false;

	if(strStr("\c0\c1\c2\c3\c4\c5\c6\c7\c8\c9", getSubStr(%title, 0, 1)) < 0)
		%message = %message @ "\c6";

	%message = %message @ %title @ "\n<font:Verdana:16>";

	for(%i = 0; strLen(%l[%i]) || strLen(%r[%i]); %i++)
	{
		if(strLen(%l[%i]))
		{
			if(%align)
				%message = %message @ "<just:left>";

			if(strStr("\c0\c1\c2\c3\c4\c5\c6\c7\c8\c9", getSubStr(%l[%i], 0, 1)) < 0)
				%message = %message @ "\c6";

			%message = %message @ %l[%i];
			%align = false;
		}

		if(strLen(%r[%i]))
		{
			if(!%align)
				%message = %message @ "<just:right>";

			if(strStr("\c0\c1\c2\c3\c4\c5\c6\c7\c8\c9", getSubStr(%r[%i], 0, 1)) < 0)
				%message = %message @ "\c6";

			%message = %message @ %r[%i] @ " ";
			%align = true;
		}

		%message = %message @ "\n";
	}

	return %message @ " ";
}



//Connecting, disconnecting, death
///////////////////////////////////////////////////////////////////////////

package NewDuplicator_Server
{
	//Set initial variables on join
	function GameConnection::onClientEnterGame(%this)
	{
		%this.ndPivot      = true;
		%this.ndLimited    = true;
		%this.ndDirection  = true;
		%this.ndForcePlant = false;

		%this.ndImage          = ND_Image.getId();
		%this.ndMode           = NDM_Disabled;
		%this.ndModeIndex      = $NDM::Disabled;
		%this.ndLastSelectMode = NDM_StackSelect;

		parent::onClientEnterGame(%this);
	}

	//Kill duplicator mode when a client leaves
	function GameConnection::onClientLeaveGame(%this)
	{
		if(%this.ndModeIndex)
			%this.ndKillMode(%this);

		%this.ndEquipped = false;

		//Remove from client lists of selections
		for(%i = 0; %i < ND_ServerGroup.getCount(); %i++)
		{
			%obj = ND_ServerGroup.getObject(%i);

			if(%obj.getName() $= "ND_Selection")
			{
				for(%j = 0; %j < %obj.numClients; %j++)
				{
					if($NS[%obj, "CL", %j] == %this.getId())
					{
						for(%k = %j; %k < (%obj.numClients - 1); %k++)
							$NS[%obj, "CL", %k] = $NS[%obj, "CL", %k + 1];

						%obj.numClients--;
						break;
					}
				}
			}
		}

		//Delete undo groups
		deleteVariables("$NU" @ %this @ "_*");

		%stack = %this.undoStack;
		%max = %stack.head;

		if(%max < %stack.tail)
			%max += %stack.size;

		for(%i = %stack.tail; %i < %max; %i++)
		{
			%val = %stack.val[%i % %start.size];

			if(getFieldCount(%val) == 2)
			{
				%str = getField(%val, 1);

				if(%str $= "ND_PLANT"
				|| %str $= "ND_PAINT"
				|| %str $= "ND_WRENCH")
				{
					%group = getField(%val, 0);

					if(isObject(%group))
					{
						%group.brickCount = 0;
						%group.delete();
					}
				}
			}
		}

		parent::onClientLeaveGame(%this);
	}

	//Kill duplicator mode when a player dies
	function GameConnection::onDeath(%this, %a, %b, %c, %d)
	{
		if(%this.ndModeIndex)
			%this.ndKillMode(%this);

		%this.ndEquipped = false;

		parent::onDeath(%this, %a, %b, %c, %d);
	}

	//Kill duplicator mode when a player is force respawned
	function GameConnection::spawnPlayer(%this)
	{
		if(%this.ndModeIndex)
			%this.ndKillMode(%this);

		%this.ndEquipped = false;

		parent::spawnPlayer(%this);
	}
};
