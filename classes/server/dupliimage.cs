// * ######################################################################
// *
// *    New Duplicator - Classes - Server 
// *    ND_DupliImage
// *
// *    -------------------------------------------------------------------
// *    Callback functions for the handheld duplicator image
// *
// * ######################################################################

//Parent functions must be called for these
package NewDuplicator_Server
{
	//Start select mode when duplicator is equipped
	function ND_DupliImage::onMount(%this, %player, %slot)
	{
		parent::onMount(%this, %player);
		%client = %player.client;

		//Player has an actual client
		if(%player.isHoleBot || %player.isSlayerBot || !isObject(%client))
			return;

		//Client is allowed to use the duplicator
		if($ND::AdminOnly && !%client.isAdmin)
		{
			commandToClient(%this, 'bottomPrint', "<font:Verdana:16>\c6Oops! The duplicator is admin only.", 5, true);
			return;
		}

		//Should resume last used select mode
		if(isObject(%client.ndLastSelectMode))
			%client.ndSetMode(%client.ndLastSelectMode);
		else
			%client.ndSetMode(NDDM_StackSelect);
	}

	//Cancel selecting when duplicator is put away. Don't cancel placing (bit of a hack)
	function ND_DupliImage::onUnMount(%this, %player, %slot)
	{
		parent::onUnMount(%this, %player);
		%client = %player.client;

		//Player has an actual client
		if(%player.isHoleBot || %player.isSlayerBot || !isObject(%client))
			return;

		//If client wasn't allowed to use the dup, he won't have a dupli mode set
		if(%client.ndModeNum)
			%client.ndSetMode(NDDM_Disabled);
	}
};

//Start the swinging animation and handle clicking stuff
function ND_DupliImage::onFire(%this, %player, %slot)
{
	%player.playThread(2, armAttack);
	%client = %player.client;

	//Player has an actual client
	if(%player.isHoleBot || %player.isSlayerBot || !isObject(%client))
		return;

	//Client is allowed to use the duplicator
	if($ND::AdminOnly && !%client.isAdmin)
		return;

	//Swinging is allowed in the current mode
	if(!%client.ndMode.allowSwinging)
		return;

	//Fire raycast in the direction the player is looking, from his camera position
	%start = %player.getEyePoint();
	%end = vectorAdd(%start, vectorScale(%player.getEyeVector(), 1000));
	%mask = $TypeMasks::FxBrickAlwaysObjectType | $TypeMasks::TerrainObjectType;

	%ray = containerRaycast(%start, %end, %mask, %player);
	%obj = firstWord(%ray);

	if(!isObject(%obj))
		return;
	
	%pos = posFromRaycast(%ray);
	%normal = normalFromRaycast(%ray);

	//Spawn the dupli emitter
	%proj = new Projectile()
	{
		datablock = ND_DupliProjectile;
		initialPosition = %pos;
	};

	%proj.explode();

	//Pass on the selected object to the dupli mode handler
	%client.ndMode.onSelectObject(%client, %obj, %pos, %normal);
}

//Stop the swinging animation
function ND_DupliImage::onStopFire(%this, %player, %slot)
{
	%player.stopThread(2);
}
