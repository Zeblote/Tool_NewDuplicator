// * ######################################################################
// *
// *    New Duplicator - Scripts - Server 
// *    Duplicator
// *
// *    -------------------------------------------------------------------
// *    Handles general functions that don't fit in a specific class
// *
// * ######################################################################

//Connecting, disconnecting, death
///////////////////////////////////////////////////////////////////////////

package NewDuplicator_Server
{
	//Set initial variables on join
	function GameConnection::onClientEnterGame(%this)
	{
		%this.ndPivot     = true;
		%this.ndLimited   = true;
		%this.ndDirection = true;

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

		//Remove from client lists of selections
		for(%i = 0; %i < ND_ServerGroup.getCount(); %i++)
		{
			%obj = ND_ServerGroup.getObject(%i);

			if(%obj.getName() $= "ND_Selection")
			{				
				%obj.numClients = 0;

				for(%j = 0; %j < ClientGroup.getCount(); %j++)
				{
					%cl = ClientGroup.getObject(%j);

					if(%cl.getId() != %this.getId()
					&& %cl.hasSpawnedOnce
					&& isObject(%ctrl = %cl.getControlObject())
					&& vectorDist(%obj.ghostPosition, %ctrl.getTransform()) < 10000)
					{
						$NS[%obj, "CL", %obj.numClients] = %cl;
						%obj.numClients++;
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



//Duplicator modes and bottomprints
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

	if(strPos("\c0\c1\c2\c3\c4\c5\c6\c7\c8\c9", getSubStr(%title, 0, 1)) < 0)
		%message = %message @ "\c6";

	%message = %message @ %title @ "\n<font:Verdana:16>";

	for(%i = 0; strLen(%l[%i]) || strLen(%r[%i]); %i++)
	{
		if(strLen(%l[%i]))
		{
			if(%align)
				%message = %message @ "<just:left>";

			if(strPos("\c0\c1\c2\c3\c4\c5\c6\c7\c8\c9", getSubStr(%l[%i], 0, 1)) < 0)
				%message = %message @ "\c6";

			%message = %message @ %l[%i];
			%align = false;
		}

		if(strLen(%r[%i]))
		{
			if(!%align)
				%message = %message @ "<just:right>";

			if(strPos("\c0\c1\c2\c3\c4\c5\c6\c7\c8\c9", getSubStr(%r[%i], 0, 1)) < 0)
				%message = %message @ "\c6";

			%message = %message @ %r[%i] @ " ";
			%align = true;
		}

		%message = %message @ "\n";
	}

	return %message @ " ";
}

package NewDuplicator_Server
{
	//Prevent accidently unequipping the duplicator
	function serverCmdUnUseTool(%client)
	{
		if(%client.ndLastEquipTime + 1.5 > $Sim::Time)
			return;

		parent::serverCmdUnUseTool(%client);
	}
	
	//Prevent creating ghost bricks in modes that allow un-mount
	function BrickDeployProjectile::onCollision(%this, %obj, %col, %fade, %pos, %normal)
	{
		%client = %obj.client;

		if(isObject(%client) && %client.ndModeIndex)
			%client.ndMode.onSelectObject(%client, %col, %pos, %normal);
		else
			parent::onCollision(%this, %obj, %col, %fade, %pos, %normal);
	}
};
