// This file should not exist. Fix later...
// -------------------------------------------------------------------

//Delete this undo group
function ND_UndoGroupWrench::onRemove(%this)
{
	if(%this.brickCount)
		deleteVariables("$NU" @ %this.client @ "_" @ %this @ "_*");
}

//Start undo wrench
function ND_UndoGroupWrench::ndStartUndo(%this, %client)
{
	%client.ndUndoInProgress = true;
	%client.ndLastMessageTime = $Sim::Time;
	%this.ndTickUndo(0, %client);
}

//Tick undo wrench
function ND_UndoGroupWrench::ndTickUndo(%this, %start, %client)
{
	%end = %start + $Pref::Server::ND::ProcessPerTick;

	if(%end > %this.brickCount)
		%end = %this.brickCount;

	setCurrentQuotaObject(getQuotaObjectFromClient(%client));

	%fillWrenchName       = %this.fillWrenchName;
	%fillWrenchLight      = %this.fillWrenchLight;
	%fillWrenchEmitter    = %this.fillWrenchEmitter;
	%fillWrenchEmitterDir = %this.fillWrenchEmitterDir;
	%fillWrenchItem       = %this.fillWrenchItem;
	%fillWrenchItemPos    = %this.fillWrenchItemPos;
	%fillWrenchItemDir    = %this.fillWrenchItemDir;
	%fillWrenchItemTime   = %this.fillWrenchItemTime;
	%fillWrenchRaycasting = %this.fillWrenchRaycasting;
	%fillWrenchCollision  = %this.fillWrenchCollision;
	%fillWrenchRendering  = %this.fillWrenchRendering;

	for(%i = %start; %i < %end; %i++)
	{
		%brick = $NU[%client, %this, "B", %i];

		if(!isObject(%brick))
			continue;

		//Revert wrench settings
		if(%fillWrenchName)
		{
			%curr = getSubStr(%brick.getName(), 1, 254);
			%fillWrenchNameValue = $NU[%client, %this, "N", %i];

			if(%curr !$= %fillWrenchNameValue)
				%brick.setNTObjectName(%fillWrenchNameValue);
		}

		if(%fillWrenchLight)
		{
			if(%tmp = %brick.light | 0)
				%curr = %tmp.getDatablock();
			else
				%curr = 0;

			%fillWrenchLightValue = $NU[%client, %this, "LDB", %i];

			if(%curr != %fillWrenchLightValue)
				%brick.setLight(%fillWrenchLightValue);
		}

		if(%fillWrenchEmitter)
		{
			if(%tmp = %brick.emitter | 0)
				%curr = %tmp.getEmitterDatablock();
			else if(%tmp = %brick.oldEmitterDB | 0)
				%curr = %tmp;
			else
				%curr = 0;

			%fillWrenchEmitterValue = $NU[%client, %this, "EDB", %i];

			if(%curr != %fillWrenchEmitterValue)
				%brick.setEmitter(%fillWrenchEmitterValue);
		}

		if(%fillWrenchEmitterDir)
		{
			%curr = %brick.emitterDirection;
			%fillWrenchEmitterDirValue = $NU[%client, %this, "EDIR", %i];

			if(%curr != %fillWrenchEmitterDirValue)
				%brick.setEmitterDirection(%fillWrenchEmitterDirValue);
		}

		if(%fillWrenchItem)
		{
			if(%tmp = %brick.item | 0)
				%curr = %tmp.getDatablock();
			else
				%curr = 0;

			%fillWrenchItemValue = $NU[%client, %this, "IDB", %i];

			if(%curr != %fillWrenchItemValue)
				%brick.setItem(%fillWrenchItemValue);
		}

		if(%fillWrenchItemPos)
		{
			%curr = %brick.itemPosition;
			%fillWrenchItemPosValue = $NU[%client, %this, "IPOS", %i];

			if(%curr != %fillWrenchItemPosValue)
				%brick.setItemPosition(%fillWrenchItemPosValue);
		}

		if(%fillWrenchItemDir)
		{
			%curr = %brick.itemPosition;
			%fillWrenchItemDirValue = $NU[%client, %this, "IDIR", %i];

			if(%curr != %fillWrenchItemDirValue)
				%brick.setItemDirection(%fillWrenchItemDirValue);
		}

		if(%fillWrenchItemTime)
		{
			%curr = %brick.itemRespawnTime;
			%fillWrenchItemTimeValue = $NU[%client, %this, "IRT", %i];

			if(%curr != %fillWrenchItemTimeValue)
				%brick.setItemRespawnTime(%fillWrenchItemTimeValue);
		}

		if(%fillWrenchRaycasting)
		{
			%curr = %brick.isRaycasting();
			%fillWrenchRaycastingValue = $NU[%client, %this, "RC", %i];

			if(%curr != %fillWrenchRaycastingValue)
				%brick.setRaycasting(%fillWrenchRaycastingValue);
		}

		if(%fillWrenchCollision)
		{
			%curr = %brick.isColliding();
			%fillWrenchCollisionValue = $NU[%client, %this, "C", %i];

			if(%curr != %fillWrenchCollisionValue)
				%brick.setColliding(%fillWrenchCollisionValue);
		}

		if(%fillWrenchRendering)
		{
			%curr = %brick.isRendering();
			%fillWrenchRenderingValue = $NU[%client, %this, "R", %i];

			if(%curr != %fillWrenchRenderingValue)
			{
				//Copy emitter ...?
				if(!%fillWrenchRenderingValue && (%tmp = %brick.emitter | 0))
					%emitter = %tmp.getEmitterDatablock();
				else
					%emitter = 0;

				%brick.setRendering(%fillWrenchRenderingValue);

				if(!%fillWrenchRenderingValue && %emitter)
					%brick.setEmitter(%emitter);
			}
		}
	}

	clearCurrentQuotaObject();

	//If undo is taking long, tell the client how far we get
	if(%client.ndLastMessageTime + 0.1 < $Sim::Time)
	{
		%client.ndLastMessageTime = $Sim::Time;

		%percent = mFloor(%end * 100 / %this.brickCount);
		commandToClient(%client, 'centerPrint', "<font:Verdana:20>\c6Undo in progress...\n<font:Verdana:17>\c3" @ %percent @ "%\c6 finished.", 10);
	}

	if(%end >= %this.brickcount)
	{
		%this.delete();
		%client.ndUndoInProgress = false;

		if(%start != 0)
			commandToClient(%client, 'centerPrint', "<font:Verdana:20>\c6Undo finished.", 2);

		return;
	}

	%this.schedule(30, ndTickUndo, %end, %client);
}
