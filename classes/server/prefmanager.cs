// * ######################################################################
// *
// *    New Duplicator - Classes - Server
// *    ND_PrefManager
// *
// *    -------------------------------------------------------------------
// *    Detect services like RTB to register preferences
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
	echo("ND_PrefManager registering RTB prefs");

	//General
	RTB_registerPref("Admin Only",                  "New Duplicator | General",     "$ND::AdminOnly",               "bool",             "Tool_NewDuplicator", false,   false, false, "");
	RTB_registerPref("Max Bricks (Admin)",          "New Duplicator | General",     "$ND::MaxBricksAdmin",          "int 1000 1000000", "Tool_NewDuplicator", 1000000, false, false, "");
	RTB_registerPref("Max Bricks (non-Admin)",      "New Duplicator | General",     "$ND::MaxBricksNonAdmin",       "int 1000 1000000", "Tool_NewDuplicator", 50000,   false, false, "");
	RTB_registerPref("Enable Menu Sounds",          "New Duplicator | General",     "$ND::PlayMenuSounds",          "bool",             "Tool_NewDuplicator", true,    false, false, "");

	//Colors
	RTB_registerPref("Highlight Color Id",          "New Duplicator | Colors",      "$ND::BrickHighlightColor",     "int 0 63",         "Tool_NewDuplicator", 3,       false, false, "");
	RTB_registerPref("Highlight Color Fx Id",       "New Duplicator | Colors",      "$ND::BrickHighlightColorFx",   "int 0 6",          "Tool_NewDuplicator", 3,       false, false, "");

	//Highlight
	RTB_registerPref("Highlight Time (ms)",         "New Duplicator | Highlight",   "$ND::HighlightTime",           "int 0 50000",      "Tool_NewDuplicator", 8000,    false, false, "");
	RTB_registerPref("De-Highlight Tick (ms)",      "New Duplicator | Highlight",   "$ND::DeHighlightTickDelay",    "int 1 50000",      "Tool_NewDuplicator", 30,      false, false, "");
	RTB_registerPref("De-Highlight per Tick",       "New Duplicator | Highlight",   "$ND::DeHighlightPerTick",      "int 1 50000",      "Tool_NewDuplicator", 400,     false, false, "");

	//Selection
	RTB_registerPref("Stack Select Tick (ms)",      "New Duplicator | Selection",   "$ND::StackSelectTickDelay",    "int 1 50000",      "Tool_NewDuplicator", 30,      false, false, "");
	RTB_registerPref("Stack Select per Tick",       "New Duplicator | Selection",   "$ND::StackSelectPerTick",      "int 1 50000",      "Tool_NewDuplicator", 400,     false, false, "");

	RTB_registerPref("Cube Select Tick (ms)",       "New Duplicator | Selection",   "$ND::CubeSelectTickDelay",     "int 1 50000",      "Tool_NewDuplicator", 30,      false, false, "");
	RTB_registerPref("Cube Select per Tick",        "New Duplicator | Selection",   "$ND::CubeSelectPerTick",       "int 1 50000",      "Tool_NewDuplicator", 400,    false, false, "");
	RTB_registerPref("Cube Select Chunk Size",      "New Duplicator | Selection",   "$ND::CubeSelectChunkSize",     "int 1 50000",      "Tool_NewDuplicator", 48,      false, false, "");

	//Ghosting
	RTB_registerPref("Scatter Ghost Bricks",        "New Duplicator | Ghosting",    "$ND::ScatterGhostBricks",      "bool",             "Tool_NewDuplicator", true,    false, false, "");

	RTB_registerPref("Max Ghost Bricks",            "New Duplicator | Ghosting",    "$ND::MaxGhostBricks",          "int 1 50000",      "Tool_NewDuplicator", 5000,    false, false, "");
	RTB_registerPref("Instant Ghost Bricks",        "New Duplicator | Ghosting",    "$ND::InstantGhostBricks",      "int 1 50000",      "Tool_NewDuplicator", 150,     false, false, "");

	RTB_registerPref("Instant Ghost Delay (ms)",    "New Duplicator | Ghosting",    "$ND::GhostBricksInitialDelay", "int 1 50000",      "Tool_NewDuplicator", 350,     false, false, "");
	RTB_registerPref("Move Ghost Bricks Tick (ms)", "New Duplicator | Ghosting",    "$ND::GhostBricksTickDelay",    "int 1 50000",      "Tool_NewDuplicator", 30,      false, false, "");
	RTB_registerPref("Move Ghost Bricks per Tick",  "New Duplicator | Ghosting",    "$ND::GhostBricksPerTick",      "int 1 50000",      "Tool_NewDuplicator", 500,     false, false, "");

	//Planting
	RTB_registerPref("Plant Bricks Tick (ms)",      "New Duplicator | Planting",    "$ND::PlantBricksTickDelay",    "int 1 50000",      "Tool_NewDuplicator", 30,      false, false, "");
	RTB_registerPref("Plant Bricks per Tick",       "New Duplicator | Planting",    "$ND::PlantBricksPerTick",      "int 1 50000",      "Tool_NewDuplicator", 400,     false, false, "");

	//Restore default prefs
	RTB_registerPref("Check to restore defaults",   "New Duplicator | Reset Prefs", "$ND::RestoreDefaultPrefs",     "bool",             "Tool_NewDuplicator", false,   false, false, "ndResetPrefs");
}

//Set default values
function ND_PrefManager::setDefaultValues(%this)
{
	echo("ND_PrefManager setting default prefs");

	//General
	$ND::AdminOnly               = false;
	$ND::MaxBricksAdmin          = 1000000;
	$ND::MaxBricksNonAdmin       = 50000;
	$ND::PlayMenuSounds          = true;

	//Colors
	$ND::BrickHighlightColor     = 3;
	$ND::BrickHighlightColorFx   = 3;

	//Highlight
	$ND::HighlightTime           = 8000;
	$ND::DeHighlightTickDelay    = 30;
	$ND::DeHighlightPerTick      = 400;

	//Selection
	$ND::StackSelectTickDelay    = 30;
	$ND::StackSelectPerTick      = 400;

	$ND::CubeSelectTickDelay     = 30;
	$ND::CubeSelectChunkSize     = 48;
	$ND::CubeSelectPerTick       = 400;

	//Ghosting
	$ND::ScatterGhostBricks      = true;

	$ND::MaxGhostBricks          = 5000;
	$ND::InstantGhostBricks      = 150;

	$ND::GhostBricksInitialDelay = 350;
	$ND::GhostBricksTickDelay    = 30;
	$ND::GhostBricksPerTick      = 500;

	//Planting
	$ND::PlantBricksTickDelay    = 30;
	$ND::PlantBricksPerTick      = 400;

	//Always set this to false
	$ND::RestoreDefaultPrefs     = false;
}

//Print prefs to client (debug, may be useful for release?)
function ND_PrefManager::dumpPrefs(%this, %client)
{
	messageClient(%client, '', "\c6New Duplicator pref values");	
	messageClient(%client, '', "\c7General");
	messageClient(%client, '', "\c6      Admin Only: \c3" @ ($ND::AdminOnly ? "Y" : "N"));
	messageClient(%client, '', "\c6      Max Bricks (Admin): \c3" @ $ND::MaxBricksAdmin);
	messageClient(%client, '', "\c6      Max Bricks (non-Admin): \c3" @ $ND::MaxBricksNonAdmin);
	messageClient(%client, '', "\c6      Enable Menu Sounds: \c3" @ ($ND::PlayMenuSounds ? "Y" : "N"));

	messageClient(%client, '', "\c7Colors");
	messageClient(%client, '', "\c6      Highlight Color: \c3" @ $ND::BrickHighlightColor);
	messageClient(%client, '', "\c6      Highlight Color Fx: \c3" @ $ND::BrickHighlightColorFx);

	messageClient(%client, '', "\c7Highlight");
	messageClient(%client, '', "\c6      Highlight Time: \c3" @ $ND::HighlightTime);
	messageClient(%client, '', "\c6      De-Highlight Tick (ms): \c3" @ $ND::DeHighlightTickDelay);
	messageClient(%client, '', "\c6      De-Highlight per Tick: \c3" @ $ND::DeHighlightPerTick);

	messageClient(%client, '', "\c7Selection");
	messageClient(%client, '', "\c6      Stack Select Tick (ms): \c3" @ $ND::StackSelectTickDelay);
	messageClient(%client, '', "\c6      Stack Select per Tick: \c3" @ $ND::StackSelectPerTick);
	messageClient(%client, '', "\c6      Cube Select Tick (ms): \c3" @ $ND::CubeSelectTickDelay);
	messageClient(%client, '', "\c6      Cube Select per Tick: \c3" @ $ND::CubeSelectPerTick);
	messageClient(%client, '', "\c6      Cube Select Chunk Size: \c3" @ $ND::CubeSelectChunkSize);

	messageClient(%client, '', "\c7Ghosting");
	messageClient(%client, '', "\c6      Scatter Ghost Bricks: \c3" @ ($ND::ScatterGhostBricks ? "Y" : "N"));
	messageClient(%client, '', "\c6      Max Ghost Bricks: \c3" @ $ND::MaxGhostBricks);
	messageClient(%client, '', "\c6      Instant Ghost Bricks: \c3" @ $ND::InstantGhostBricks);
	messageClient(%client, '', "\c6      Instant Ghost Delay (ms): \c3" @ $ND::GhostBricksInitialDelay);
	messageClient(%client, '', "\c6      Move Ghost Bricks Tick (ms): \c3" @ $ND::GhostBricksTickDelay);
	messageClient(%client, '', "\c6      Move Ghost Bricks per Tick: \c3" @ $ND::GhostBricksPerTick);

	messageClient(%client, '', "\c7Planting");
	messageClient(%client, '', "\c6      Plant Bricks Tick (ms): \c3" @ $ND::PlantBricksTickDelay);
	messageClient(%client, '', "\c6      Plant Bricks per Tick: \c3" @ $ND::PlantBricksPerTick);
}

//Callback function to restore default prefs
function ndResetPrefs()
{
	if($ND::RestoreDefaultPrefs)
		ND_PrefManager.setDefaultValues();
}
