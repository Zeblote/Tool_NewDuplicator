// * ######################################################################
// *
// *    New Duplicator - Classes - Server
// *    ND_AsyncTask
// *
// *    -------------------------------------------------------------------
// *    Abstract class for scheduling long tasks
// *
// * ######################################################################

//Create task group
if(isObject(ND_TaskGroup))
	ND_TaskGroup.delete();

ND_ServerGroup.add(
	new ScriptGroup(ND_TaskGroup)
);

//Init some variables
function ND_AsyncTask::onAdd(%this)
{
	%this.started = false;
	%this.delayed = false;
	%this.paused = false;
	%this.ticks = 0;

	%this.callbacks = 0;
	%this.chainTasks = 0;
}

//Start task immediately, calls first tick
function ND_AsyncTask::start(%this)
{
	//echo("Start: " @ %this.class SPC %this);

	//Initial tick
	%this.delayed = false;
	%this.started = true;
	%this.tick();

	return %this;
}

//Start task after some time
function ND_AsyncTask::startDelayed(%this, %delay)
{
	//echo("Delay: " @ %this.class SPC %this @ ", delay " @ %delay);

	//Initial tick
	%this.delayed = true;

	cancel(%this.schedule);
	%this.schedule = %this.schedule(%delay, start);

	return %this;
}

//Skip starting delay and tick immediately
function ND_AsyncTask::skipDelay(%this)
{
	if(%this.delayed)
	{
		//echo("Skip Delay: " @ %this.class SPC %this @ ", time left " @ getTimeRemaining(%this.schedule));
		%this.start();
	}
}

//Schedule the next tick (parent must be called!)
function ND_AsyncTask::tick(%this)
{
	//echo("Tick: " @ %this.class SPC %this @ ", tick " @ %this.ticks);
	%this.ticks++;

	//Next tick
	cancel(%this.schedule);
	%this.schedule = %this.schedule(%this.tickTime, tick);
}

//Pause the task
function ND_AsyncTask::pause(%this)
{
	//echo("Pause: " @ %this.class SPC %this @ ", next tick " @ %this.ticks);

	%this.paused = true;
	cancel(%this.schedule);
}

//Resume paused task
function ND_AsyncTask::resume(%this)
{
	if(%this.paused)
	{
		//echo("Resume: " @ %this.class SPC %this @ ", next tick " @ %this.ticks);

		%this.paused = false;
		this.tick();
	}
}

//Cancel this task and delete chained tasks
function ND_AsyncTask::cancel(%this)
{
	//echo("Cancel: " @ %this.class SPC %this @ ", " @ %this.ticks @ " tick(s)");

	cancel(%this.schedule);
	%this.delete();
}

//Finish the task (must be manually called)
function ND_AsyncTask::finish(%this)
{
	//echo("Finished: " @ %this.class SPC %this @ ", " @ %this.ticks @ " tick(s)");

	//First execute callbacks
	for(%i = 0; %i < %this.callbacks; %i++)
		eval(%this.callbackString[%i]);

	//Then delete this task
	cancel(%this.schedule);
	%this.delete();
}

//Add a callback for this task
function ND_AsyncTask::addCallback(%this, %target, %method, %a0, %a1, %a2, %a3, %a4, %a5)
{
	//echo("Callback: " @ %this.class SPC %this @ ", " @ %target SPC %method SPC %a0 SPC %a1 SPC %a2 SPC %a3 SPC %a4 SPC %a5);

	if(strLen(%a0))
	{
		%args = "\"" @ expandEscape(%a0) @ "\"";

		for(%i = 1; strLen(%a[%i]); %i++)
			%args = %args @ ", \"" @ expandEscape(%a[%i]) @ "\"";
	}

	if(strLen(%target))
		%string = %target.getID() @ "." @ %method @ "(" @ %args @ ");";
	else
		%string = %method @ "(" @ %args @ ");";

	%this.callbackTarget[%this.callbacks] = %target;
	%this.callbackMethod[%this.callbacks] = %method;
	%this.callbackString[%this.callbacks] = %string;

	%this.callbacks++;
}

//Remove a callback from this task by matching the target and method
function ND_AsyncTask::removeCallback(%this, %target, %method)
{
	//echo("Remove CB: " @ %this.class SPC %this @ ", " @ %target SPC %method);

	for(%i = 0; %i < %this.callbacks; %i++)
	{
		if(%this.callbackTarget[%i] $= %target && %this.callbackMethod[%i] $= %method)
		{
			for(%j = %i; %j < %this.callbacks - 1; %j++)
			{
				%this.callbackTarget[%j] = %this.callbackTarget[%j + 1];
				%this.callbackMethod[%j] = %this.callbackMethod[%j + 1];
				%this.callbackString[%j] = %this.callbackString[%j + 1];
			}

			%this.callbackTarget[%j] = "";
			%this.callbackMethod[%j] = "";
			%this.callbackString[%j] = "";

			%this.callbacks--;

			break;
		}
	}
}
