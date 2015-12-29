// * ######################################################################
// *
// *    New Duplicator - Classes - Server 
// *    ND_Image
// *
// *    -------------------------------------------------------------------
// *    Handles equipping and using a new duplicator
// *
// * ######################################################################

//Set which image a client should use
function GameConnection::ndSetImage(%this, %image)
{
	%image = %image.getId();

	if(%image != %this.ndImage)
	{
		%this.ndImage = %image;

		if(%this.ndEquipped)
		{
			%this.ndIgnoreNextMount = true;
			%this.player.schedule(0, updateArm, %image);
			%this.player.schedule(0, mountImage, %image, 0);
		}
	}
}

//Mount the correct image when the item is equipped
function ND_Item::onUse(%this, %player, %slot)
{
	%image = %player.client.ndImage;

	if(!isObject(%image))
		%image = ND_Image;

	%player.updateArm(%image);
	%player.mountImage(%image, 0);
}

package NewDuplicator_Server
{
	//Start select mode when duplicator is equipped
	function ND_Image::onMount(%this, %player, %slot)
	{
		parent::onMount(%this, %player, %slot);
		%player.ndEquipped();
	}

	function ND_Image_Blue::onMount(%this, %player, %slot)
	{
		parent::onMount(%this, %player, %slot);
		%player.ndEquipped();
	}

	function ND_Image_Cube::onMount(%this, %player, %slot)
	{
		parent::onMount(%this, %player, %slot);
		%player.ndEquipped();
	}

	//Cancel mode when duplicator is unequipped
	function ND_Image::onUnMount(%this, %player, %slot)
	{
		parent::onUnMount(%this, %player, %slot);
		%player.ndUnEquipped();
	}

	function ND_Image_Blue::onUnMount(%this, %player, %slot)
	{
		parent::onUnMount(%this, %player, %slot);
		%player.ndUnEquipped();
	}

	function ND_Image_Cube::onUnMount(%this, %player, %slot)
	{
		parent::onUnMount(%this, %player, %slot);
		%player.ndUnEquipped();
	}
};

//Start the swinging animation
function ND_Image::onPreFire(%this, %player, %slot)
{
	%player.playThread(2, shiftTo);
}

function ND_Image_Blue::onPreFire(%this, %player, %slot)
{
	%player.playThread(2, shiftTo);
}

function ND_Image_Cube::onPreFire(%this, %player, %slot)
{
	%player.playThread(2, shiftTo);
}

//Handle selecting things
function ND_Image::onFire(%this, %player, %slot)
{
	%player.ndFired();
}

function ND_Image_Blue::onFire(%this, %player, %slot)
{
	%player.ndFired();
}

function ND_Image_Cube::onFire(%this, %player, %slot)
{
	%player.ndFired();
}

//Duplicator was equipped
function Player::ndEquipped(%this)
{
	%client = %this.client;

	if(%this.isHoleBot || %this.isSlayerBot || !isObject(%client))
		return;

	if(%client.ndIgnoreNextMount)
	{
		%client.ndIgnoreNextMount = false;
		return;
	}

	if($Pref::Server::ND::AdminOnly && !%client.isAdmin)
	{
		commandToClient(%client, 'centerPrint', "<font:Verdana:20>\c6Oops! The duplicator is admin only.", 5);
		return;
	}

	%client.ndEquipped = true;

	//Remove temp brick so it doesn't overlap the selection box
	if(isObject(%this.tempBrick))
		%this.tempBrick.delete();

	//Should resume last used select mode
	if(!%client.ndModeIndex)
		%client.ndSetMode(%client.ndLastSelectMode);
}

//Duplicator was unequipped
function Player::ndUnEquipped(%this)
{
	%client = %this.client;

	if(%this.isHoleBot || %this.isSlayerBot || !isObject(%client))
		return;

	if(%client.ndIgnoreNextMount)
		return;

	if(%client.ndModeIndex && !%client.ndMode.allowUnMount)
		%client.ndKillMode();

	%client.ndEquipped = false;
}

//Duplicator was fired
function Player::ndFired(%this)
{
	%client = %this.client;

	if(!isObject(%client) || !%client.ndModeIndex || !%client.ndMode.allowSelecting)
		return;

	%minigame = getMinigameFromObject(%client);

	if(isObject(%minigame) && !%minigame.enablebuilding)
	{
		commandToClient(%client, 'centerPrint', "<font:Verdana:20>\c6Oops! Building is disabled.", 5);
		return;
	}

	//Fire raycast in the direction the player is looking, from his camera position
	%start = %this.getEyePoint();
	%end = vectorAdd(%start, vectorScale(%this.getEyeVector(), 1000));

	%mask = $TypeMasks::FxBrickAlwaysObjectType | $TypeMasks::TerrainObjectType;
	%ray = containerRaycast(%start, %end, %mask, %this);

	if(!isObject(%obj = firstWord(%ray)))
		return;
	
	%position = posFromRaycast(%ray);
	%normal = normalFromRaycast(%ray);

	//Can't directly spawn an explosion, must use a projectile
	%data = %client.ndImage.projectile;

	if(!isObject(%data))
		%data = ND_HitProjectile;

	%proj = new Projectile()
	{
		datablock = %data;
		initialPosition = %position;
		initialVelocity = %normal;
	};

	//Pass on the selected object to the dupli mode
	%client.ndMode.onSelectObject(%client, %obj, %position, %normal);
}

package NewDuplicator_Server
{
	//Automatically start the "ambient" animation on duplicator items
	function ND_Item::onAdd(%this, %obj)
	{
		parent::onAdd(%this, %obj);
		%obj.playThread(0, ambient);

		//Fix colorshift bullshit
		%obj.schedule(100, setNodeColor, "ALL", %this.colorShiftColor);
	}

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
