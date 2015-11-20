// * ######################################################################
// *
// *    New Duplicator - Classes - Server 
// *    NewDuplicatorImage
// *
// *    -------------------------------------------------------------------
// *    Handles equipping and using a new duplicator
// *
// * ######################################################################

//Mount the correct color when the item is equipped
function NewDuplicatorItem::onUse(%this, %player, %slot)
{
	if(%player.client.ndBlueImage)
		%image = NewDuplicatorBlueImage.getId();
	else
		%image = NewDuplicatorImage.getId();

	%player.updateArm(%image);
	%player.mountImage(%image, 0);
}

package NewDuplicator_Server
{
	//Start select mode when duplicator is equipped
	function NewDuplicatorImage::onMount(%this, %player, %slot)
	{
		parent::onMount(%this, %player, %slot);
		%client = %player.client;

		if(%player.isHoleBot || %player.isSlayerBot || !isObject(%client))
			return;

		%client.ndEquipped();
	}

	//Blue duplicator
	function NewDuplicatorBlueImage::onMount(%this, %player, %slot)
	{
		parent::onMount(%this, %player, %slot);
		%client = %player.client;

		if(%player.isHoleBot || %player.isSlayerBot || !isObject(%client))
			return;
		
		%client.ndEquipped();
	}

	//Cancel mode when duplicator is unequipped
	function NewDuplicatorImage::onUnMount(%this, %player, %slot)
	{
		parent::onUnMount(%this, %player, %slot);
		%client = %player.client;

		if(%player.isHoleBot || %player.isSlayerBot || !isObject(%client))
			return;

		%client.ndUnEquipped();
	}

	//Blue duplicator
	function NewDuplicatorBlueImage::onUnMount(%this, %player, %slot)
	{
		parent::onUnMount(%this, %player, %slot);
		%client = %player.client;

		if(%player.isHoleBot || %player.isSlayerBot || !isObject(%client))
			return;

		%client.ndUnEquipped();
	}
};

//Start the swinging animation and handle clicking things
function NewDuplicatorImage::onFire(%this, %player, %slot)
{
	%player.playThread(2, armAttack);
	%client = %player.client;

	if(!%client.ndModeIndex || !%client.ndMode.allowSelecting)
		return;

	%client.ndFired();
}

//Blue duplicator
function NewDuplicatorBlueImage::onFire(%this, %player, %slot)
{
	%player.playThread(2, armAttack);
	%client = %player.client;

	if(!%client.ndModeIndex || !%client.ndMode.allowSelecting)
		return;

	%client.ndFired();
}

//Stop the swinging animation
function NewDuplicatorImage::onStopFire(%this, %player, %slot)
{
	%player.stopThread(2);
}

//Blue duplicator
function NewDuplicatorBlueImage::onStopFire(%this, %player, %slot)
{
	NewDuplicatorImage.onStopFire(%player, %slot);
}

//Set whether a client should use the blue image
function GameConnection::ndSetBlueImage(%this, %bool)
{
	%this.ndBlueImage = %bool;
	%currImage = %this.player.getMountedImage(0);

	if(%bool && %currImage == NewDuplicatorImage.getId())
	{
		%this.ndIgnoreMount = true;
		%this.player.updateArm(NewDuplicatorBlueImage);
		%this.player.mountImage(NewDuplicatorBlueImage, 0);	
	}
	else if (!%bool && %currImage == NewDuplicatorBlueImage.getId())
	{
		%this.ndIgnoreMount = true;
		%this.player.updateArm(NewDuplicatorImage);
		%this.player.mountImage(NewDuplicatorImage, 0);		
	}
}

//Duplicator was equipped
function GameConnection::ndEquipped(%this)
{
	if(%client.ndIgnoreMount)
	{
		%client.ndIgnoreMount = false;
		return;
	}

	if($Pref::Server::ND::AdminOnly && !%this.isAdmin)
	{
		commandToClient(%this, 'centerPrint', "<font:Verdana:20>\c6Oops! The duplicator is admin only.", 5);
		return;
	}

	//Remove temp brick so it doesn't overlap the selection box
	if(isObject(%this.player.tempBrick))
		%this.player.tempBrick.delete();

	//Should resume last used select mode
	if(!%this.ndModeIndex)
		%this.ndSetMode(%this.ndLastSelectMode);

	%this.ndEquipped = true;
}

//Duplicator was unequipped
function GameConnection::ndUnEquipped(%this)
{
	if(%client.ndIgnoreMount)
		return;

	if(%this.ndModeIndex && !%this.ndMode.allowUnMount)
		%this.ndKillMode();

	%this.ndEquipped = false;
}

//Duplicator was fired
function GameConnection::ndFired(%this)
{
	%player = %this.player;

	//Fire raycast in the direction the player is looking, from his camera position
	%start = %player.getEyePoint();
	%end = vectorAdd(%start, vectorScale(%player.getEyeVector(), 1000));

	%mask = $TypeMasks::FxBrickAlwaysObjectType | $TypeMasks::TerrainObjectType;
	%ray = containerRaycast(%start, %end, %mask, %player);

	if(!isObject(%obj = firstWord(%ray)))
		return;
	
	%position = posFromRaycast(%ray);
	%normal = normalFromRaycast(%ray);

	//Can't directly spawn an explosion, must use a projectile
	if(%this.ndBlueImage)
		%data = NewDuplicatorBlueProjectile;
	else
		%data = NewDuplicatorProjectile;

	%proj = new Projectile()
	{
		datablock = %data;
		initialPosition = %position;
	};

	%proj.explode();

	//Pass on the selected object to the dupli mode
	%this.ndMode.onSelectObject(%this, %obj, %position, %normal);
}
