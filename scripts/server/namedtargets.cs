// Improved versions of setNTObjectName and clearNTObjectName with much
// higher performance. Required to fix lag when clearing named bricks.
// -------------------------------------------------------------------

function SimObject::setNTObjectName(%this, %name)
{
	%this = %this.getId();
	%name = getSafeVariableName(trim(%name));

	if(%name $= "")
	{
		%this.clearNTObjectName();
		%this.setName("");
		return;
	}

	//Names must start with a _ to prevent overwriting real objects
	if(getSubStr(%name, 0, 1) !$= "_")
		%name = "_" @ %name;

	if(%this.getName() $= %name)
		return;

	if(isObject(%name) && !(%name.getType() & $TypeMasks::FxBrickAlwaysObjectType))
	{
		error("ERROR: SimObject::setNTObjectName() - Non-Brick object named \"" @ %name @ "\" already exists!");
		return;
	}

	%this.clearNTObjectName();

	%group = %this.getGroup();
	%count = %group.NTObjectCount[%name] | 0;

	if(!%count)
		%group.addNTName(%name);

	//Add a reverse lookup to remove the name much faster
	%group.NTObject[%name, %count] = %this;
	%group.NTObjectIndex[%name, %this] = %count;
	%group.NTObjectCount[%name] = %count + 1;

	%this.setName(%name);
}

function SimObject::clearNTObjectName(%this)
{
	%this = %this.getId();
	%group = %this.getGroup();

	if(!isObject(%group))
		return;

	%oldName = %this.getName();

	if(%oldName $= "")
		return;

	%index = %group.NTObjectIndex[%oldName, %this];
	%count = %group.NTObjectCount[%oldName];

	if(%group.NTObject[%oldName, %index] == %this)
	{
		//Reverse lookup works, use fast version
		%lastObj = %group.NTObject[%oldName, %count - 1];
		%group.NTObject[%oldName, %index] = %lastObj;
		%group.NTObject[%oldName, %count - 1] = "";
		%group.NTObjectIndex[%oldName, %lastObj] = %index;
		%group.NTObjectIndex[%oldName, %this] = "";
		%group.NTObjectCount[%oldName]--;
	}
	else
	{
		//Reverse lookup failed, use old and slow version
		for(%i = 0; %i < %count; %i++)
		{
			if(%group.NTObject[%oldName, %i] == %this)
			{
				%group.NTObject[%oldName, %i] = %group.NTObject[%oldName, %count - 1];
				%group.NTObject[%oldName, %count - 1] = "";
				%group.NTObjectCount[%oldName]--;
				break;
			}
		}
	}

	if(!%group.NTObjectCount[%oldName])
		%group.removeNTName(%oldName);
}

