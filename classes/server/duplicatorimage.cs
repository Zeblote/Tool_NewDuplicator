// * ######################################################################
// *
// *    New Duplicator - Classes - Server 
// *    NewDuplicatorImage
// *
// *    -------------------------------------------------------------------
// *    Handles equipping and using a new duplicator
// *
// * ######################################################################

package NewDuplicator_Server
{
	//Start select mode when duplicator is equipped
	function NewDuplicatorImage::onMount(%this, %player, %slot)
	{
		parent::onMount(%this, %player, %slot);
		%client = %player.client;

		if(%player.isHoleBot || %player.isSlayerBot || !isObject(%client))
			return;

		if($ND::AdminOnly && !%client.isAdmin)
		{
			commandToClient(%this, 'centerPrint', "<font:Verdana:20>\c6Oops! The duplicator is admin only.", 5);
			return;
		}

		//Remove temp brick so it doesn't overlap the selection box
		if(isObject(%player.tempBrick))
			%player.tempBrick.delete();

		//Should resume last used select mode
		if(!%client.ndModeIndex)
			%client.ndSetMode(%client.ndLastSelectMode);

		%client.ndEquipped = true;
	}

	//Cancel mode when duplicator is unequipped
	function NewDuplicatorImage::onUnMount(%this, %player, %slot)
	{
		parent::onUnMount(%this, %player, %slot);
		%client = %player.client;

		if(%player.isHoleBot || %player.isSlayerBot || !isObject(%client))
			return;

		if(%client.ndModeIndex && !%client.ndMode.allowUnMount)
			%client.ndKillMode();

		%client.ndEquipped = false;
	}
};

//Start the swinging animation and handle clicking things
function NewDuplicatorImage::onFire(%this, %player, %slot)
{
	%player.playThread(2, armAttack);
	%client = %player.client;

	if(!%client.ndModeIndex || !%client.ndMode.allowSelecting)
		return;

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
	%proj = new Projectile()
	{
		datablock = NewDuplicatorProjectile;
		initialPosition = %position;
	};

	%proj.explode();

	//Pass on the selected object to the dupli mode
	%client.ndMode.onSelectObject(%client, %obj, %position, %normal);
}

//Stop the swinging animation
function NewDuplicatorImage::onStopFire(%this, %player, %slot)
{
	%player.stopThread(2);
}
