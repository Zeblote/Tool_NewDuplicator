// * ######################################################################
// *
// *    New Duplicator - Classes - Server
// *    ND_PrefManager
// *
// *    -------------------------------------------------------------------
// *    Detect services like RTB and Blockland Glass to register preferences
// *
// * ######################################################################

//Create pref manager
function ND_PrefManager()
{
	ND_ServerGroup.add(
		%this = new ScriptObject(ND_PrefManager)
	);

	return %this;
}

//Detect pref service and register prefs
function ND_PrefManager::registerPrefs(%this)
{
	//RTB is always executed first. If this is set, register prefs to RTB
	if($RTB::Hooks::ServerControl)
		%this.registerRTBPrefs();

	//No known pref service detected. Set defaults
	else
		%this.setDefaultValues();
}

//Register preferences to RTB
function ND_PrefManager::registerRTBPrefs(%this)
{
	echo("Registering RTB prefs");

	//Admin limits
	RTB_registerPref("Admin Only", "New Duplicator", "$ND::AdminOnly", "bool", "Tool_NewDuplicator", false, false, false, "");

	//Highlight color controls
	RTB_registerPref("Highlight Color", "New Duplicator", "$ND::BrickHighlightColor", "int 0 63", "Tool_NewDuplicator", 3, false, false, "");
	RTB_registerPref("Highlight Color Fx", "New Duplicator", "$ND::BrickHighlightColorFx", "int 0 4", "Tool_NewDuplicator", 3, false, false, "");

	//De-highlight tick controls
	RTB_registerPref("Highlight Time", "New Duplicator", "$ND::HighlightTime", "int 0 20000", "Tool_NewDuplicator", 8000, false, false, "");
	RTB_registerPref("De-Highlight Tick Delay", "New Duplicator", "$ND::DeHighlightTickDelay", "int 1 10000", "Tool_NewDuplicator", 30, false, false, "");
	RTB_registerPref("De-Highlight per Tick", "New Duplicator", "$ND::DeHighlightPerTick", "int 1 10000", "Tool_NewDuplicator", 500, false, false, "");

	//Selection tick controls
	RTB_registerPref("Stack Select Tick Delay", "New Duplicator", "$ND::StackSelectTickDelay", "int 1 10000", "Tool_NewDuplicator", 30, false, false, "");
	RTB_registerPref("Stack Select per Tick", "New Duplicator", "$ND::StackSelectPerTick", "int 1 10000", "Tool_NewDuplicator", 500, false, false, "");

	//Ghost bricks
	RTB_registerPref("Ghost by Selection Order", "New Duplicator", "$ND::GhostBySelectionOrder", "bool", "Tool_NewDuplicator", false, false, false, "");

	RTB_registerPref("Instant Ghost Bricks", "New Duplicator", "$ND::InstantGhostBricks", "int 1 10000", "Tool_NewDuplicator", 150, false, false, "");
	RTB_registerPref("Max Ghost Bricks", "New Duplicator", "$ND::MaxGhostBricks", "int 1 20000", "Tool_NewDuplicator", 5000, false, false, "");

	RTB_registerPref("Move Ghost Bricks Delay", "New Duplicator", "$ND::GhostBricksInitialDelay", "int 1 20000", "Tool_NewDuplicator", 350, false, false, "");
	RTB_registerPref("Move Ghost Bricks Tick Delay", "New Duplicator", "$ND::GhostBricksTickDelay", "int 1 10000", "Tool_NewDuplicator", 30, false, false, "");
	RTB_registerPref("Move Ghost Bricks per Tick", "New Duplicator", "$ND::GhostBricksPerTick", "int 1 10000", "Tool_NewDuplicator", 1000, false, false, "");

}

//Set default values
function ND_PrefManager::setDefaultValues(%this)
{
	echo("Setting default prefs");

	//Admin limits
	$ND::AdminOnly = false;

	//Highlight color controls
	$ND::BrickHighlightColor = 3;
	$ND::BrickHighlightColorFx = 3;

	//De-highlight tick controls
	$ND::HighlightTime = 6000;
	$ND::DeHighlightTickDelay = 30;
	$ND::DeHighlightPerTick = 200;

	//Selection tick controls
	$ND::StackSelectTickDelay = 30;
	$ND::StackSelectPerTick = 200;

	//Ghosting controls
	$ND::GhostBySelectionOrder = false;

	$ND::InstantGhostBricks = 150;
	$ND::MaxGhostBricks = 5000;

	$ND::GhostBricksInitialDelay = 350;
	$ND::GhostBricksTickDelay = 30;
	$ND::GhostBricksPerTick = 1000;
}

