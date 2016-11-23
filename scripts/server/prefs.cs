// Detects common services like RTB and registers perferences to them.
// -------------------------------------------------------------------

function ndRegisterPrefs()
{
	//Glass prefs also set this variable so we don't need to add them seperately
	if($RTB::Hooks::ServerControl)
		ndRegisterPrefsToRtb();
	else
		ndExtendDefaultPrefValues();

	ndDeleteOutdatedPrefs();
}

function ndRegisterPrefsToRtb()
{
	echo("ND: Registering RTB prefs");
	%trustDropDown = "list None 0 Build 1 Full 2 Self 3";

	//Limits
	RTB_registerPref("Admin Only",                  "New Duplicator | Limits",   "$Pref::Server::ND::AdminOnly",           "bool",             "Tool_NewDuplicator", false,   false, false, "");
	RTB_registerPref("Fill Paint Admin Only",       "New Duplicator | Limits",   "$Pref::Server::ND::PaintAdminOnly",      "bool",             "Tool_NewDuplicator", false,   false, false, "");
	RTB_registerPref("Fill Paint Fx Admin Only",    "New Duplicator | Limits",   "$Pref::Server::ND::PaintFxAdminOnly",    "bool",             "Tool_NewDuplicator", true,    false, false, "");
	RTB_registerPref("Fill Wrench Admin Only",      "New Duplicator | Limits",   "$Pref::Server::ND::WrenchAdminOnly",     "bool",             "Tool_NewDuplicator", true,    false, false, "");
	RTB_registerPref("Floating Bricks Admin Only",  "New Duplicator | Limits",   "$Pref::Server::ND::FloatAdminOnly",      "bool",             "Tool_NewDuplicator", true,    false, false, "");
	RTB_registerPref("Save Admin Only",             "New Duplicator | Limits",   "$Pref::Server::ND::SaveAdminOnly",       "bool",             "Tool_NewDuplicator", true,    false, false, "");
	RTB_registerPref("Load Admin Only",             "New Duplicator | Limits",   "$Pref::Server::ND::LoadAdminOnly",       "bool",             "Tool_NewDuplicator", false,   false, false, "");
	RTB_registerPref("Fill Bricks Admin Only",      "New Duplicator | Limits",   "$Pref::Server::ND::FillBricksAdminOnly", "bool",             "Tool_NewDuplicator", true,    false, false, "");

	//Settings
	RTB_RegisterPref("Trust Limit",                 "New Duplicator | Settings", "$Pref::Server::ND::TrustLimit",          %trustDropDown,     "Tool_NewDuplicator", 2,       false, false, "");
	RTB_RegisterPref("Admin Trust Bypass (Select)", "New Duplicator | Settings", "$Pref::Server::ND::AdminTrustBypass1",   "bool",             "Tool_NewDuplicator", true,    false, false, "");
	RTB_RegisterPref("Admin Trust Bypass (Edit)",   "New Duplicator | Settings", "$Pref::Server::ND::AdminTrustBypass2",   "bool",             "Tool_NewDuplicator", false,   false, false, "");
	RTB_RegisterPref("Select Public Bricks",        "New Duplicator | Settings", "$Pref::Server::ND::SelectPublicBricks",  "bool",             "Tool_NewDuplicator", true,    false, false, "");

	RTB_registerPref("Max Bricks (Admin)",          "New Duplicator | Settings", "$Pref::Server::ND::MaxBricksAdmin",      "int 1000 1000000", "Tool_NewDuplicator", 1000000, false, false, "");
	RTB_registerPref("Max Bricks (Player)",         "New Duplicator | Settings", "$Pref::Server::ND::MaxBricksPlayer",     "int 1000 1000000", "Tool_NewDuplicator", 50000,   false, false, "");
	RTB_registerPref("Max Box Size (Admin)",        "New Duplicator | Settings", "$Pref::Server::ND::MaxBoxSizeAdmin",     "int 1 50000",      "Tool_NewDuplicator", 1024,    false, false, "");
	RTB_registerPref("Max Box Size (Player)",       "New Duplicator | Settings", "$Pref::Server::ND::MaxBoxSizePlayer",    "int 1 50000",      "Tool_NewDuplicator", 64,      false, false, "");

	RTB_registerPref("Selecting Timeout (Player)",  "New Duplicator | Settings", "$Pref::Server::ND::SelectTimeoutMS",     "int 0 5000",       "Tool_NewDuplicator", 400,     false, false, "");
	RTB_registerPref("Planting Timeout (Player)",   "New Duplicator | Settings", "$Pref::Server::ND::PlantTimeoutMS",      "int 0 5000",       "Tool_NewDuplicator", 400,     false, false, "");

	//Advanced
	RTB_registerPref("Enable Menu Sounds",          "New Duplicator | Advanced", "$Pref::Server::ND::PlayMenuSounds",      "bool",             "Tool_NewDuplicator", true,    false, false, "");
	RTB_registerPref("Max Ghost Bricks",            "New Duplicator | Advanced", "$Pref::Server::ND::MaxGhostBricks",      "int 1 50000",      "Tool_NewDuplicator", 1500,    false, false, "");
	RTB_registerPref("Instant Ghost Bricks",        "New Duplicator | Advanced", "$Pref::Server::ND::InstantGhostBricks",  "int 1 50000",      "Tool_NewDuplicator", 150,     false, false, "");
	RTB_registerPref("Scatter Ghost Bricks",        "New Duplicator | Advanced", "$Pref::Server::ND::ScatterGhostBricks",  "bool",             "Tool_NewDuplicator", true,    false, false, "");
	RTB_registerPref("Process Bricks per Tick",     "New Duplicator | Advanced", "$Pref::Server::ND::ProcessPerTick",      "int 1 50000",      "Tool_NewDuplicator", 300,     false, false, "");
	RTB_registerPref("Box Selection Chunk Size",    "New Duplicator | Advanced", "$Pref::Server::ND::BoxSelectChunkDim",   "int 1 50000",      "Tool_NewDuplicator", 6,       false, false, "");
	RTB_registerPref("Create Sym Table on Start",   "New Duplicator | Advanced", "$Pref::Server::ND::SymTableOnStart",     "bool",             "Tool_NewDuplicator", false,   false, false, "");

	//Restore default prefs
	RTB_registerPref("Check to restore defaults", "New Duplicator | Reset Prefs", "$ND::RestoreDefaultPrefs", "bool", "Tool_NewDuplicator", false, false, false, "ndRestoreDefaultPrefs");
}

//Callback function for "Reset Prefs"
function ndRestoreDefaultPrefs()
{
	if($ND::RestoreDefaultPrefs)
		ndApplyDefaultPrefValues();
}

function ndExtendDefaultPrefValues()
{
	echo("ND: Extending default pref values");

	//Limits
	if($Pref::Server::ND::AdminOnly           $= "") $Pref::Server::ND::AdminOnly           = false;
	if($Pref::Server::ND::PaintAdminOnly      $= "") $Pref::Server::ND::PaintAdminOnly      = false;
	if($Pref::Server::ND::PaintFxAdminOnly    $= "") $Pref::Server::ND::PaintFxAdminOnly    = true;
	if($Pref::Server::ND::WrenchAdminOnly     $= "") $Pref::Server::ND::WrenchAdminOnly     = true;
	if($Pref::Server::ND::FloatAdminOnly      $= "") $Pref::Server::ND::FloatAdminOnly      = true;
	if($Pref::Server::ND::SaveAdminOnly       $= "") $Pref::Server::ND::SaveAdminOnly       = true;
	if($Pref::Server::ND::LoadAdminOnly       $= "") $Pref::Server::ND::LoadAdminOnly       = false;
	if($Pref::Server::ND::FillBricksAdminOnly $= "") $Pref::Server::ND::FillBricksAdminOnly = true;

	//Settings
	if($Pref::Server::ND::TrustLimit          $= "") $Pref::Server::ND::TrustLimit          = 2;
	if($Pref::Server::ND::AdminTrustBypass1   $= "") $Pref::Server::ND::AdminTrustBypass1   = true;
	if($Pref::Server::ND::AdminTrustBypass2   $= "") $Pref::Server::ND::AdminTrustBypass2   = false;
	if($Pref::Server::ND::SelectPublicBricks  $= "") $Pref::Server::ND::SelectPublicBricks  = true;

	if($Pref::Server::ND::MaxBricksAdmin      $= "") $Pref::Server::ND::MaxBricksAdmin      = 1000000;
	if($Pref::Server::ND::MaxBricksPlayer     $= "") $Pref::Server::ND::MaxBricksPlayer     = 10000;
	if($Pref::Server::ND::MaxBoxSizeAdmin     $= "") $Pref::Server::ND::MaxBoxSizeAdmin     = 1024;
	if($Pref::Server::ND::MaxBoxSizePlayer    $= "") $Pref::Server::ND::MaxBoxSizePlayer    = 64;

	if($Pref::Server::ND::SelectTimeoutMS     $= "") $Pref::Server::ND::SelectTimeoutMS     = 400;
	if($Pref::Server::ND::PlantTimeoutMS      $= "") $Pref::Server::ND::PlantTimeoutMS      = 400;

	//Advanced
	if($Pref::Server::ND::PlayMenuSounds      $= "") $Pref::Server::ND::PlayMenuSounds      = true;
	if($Pref::Server::ND::MaxGhostBricks      $= "") $Pref::Server::ND::MaxGhostBricks      = 1500;
	if($Pref::Server::ND::InstantGhostBricks  $= "") $Pref::Server::ND::InstantGhostBricks  = 150;
	if($Pref::Server::ND::ScatterGhostBricks  $= "") $Pref::Server::ND::ScatterGhostBricks  = true;
	if($Pref::Server::ND::ProcessPerTick      $= "") $Pref::Server::ND::ProcessPerTick      = 300;
	if($Pref::Server::ND::BoxSelectChunkDim   $= "") $Pref::Server::ND::BoxSelectChunkDim   = 6;
	if($Pref::Server::ND::SymTableOnStart     $= "") $Pref::Server::ND::SymTableOnStart     = false;

	//Always set this to false so we don't accidently reset the prefs
	$ND::RestoreDefaultPrefs = false;
}

function ndApplyDefaultPrefValues()
{
	echo("ND: Applying default pref values");
	messageAll('', "\c6(\c3New Duplicator\c6) \c6Prefs reset to default values.");

	//Limits
	$Pref::Server::ND::AdminOnly           = false;
	$Pref::Server::ND::PaintAdminOnly      = false;
	$Pref::Server::ND::PaintFxAdminOnly    = true;
	$Pref::Server::ND::WrenchAdminOnly     = true;
	$Pref::Server::ND::FloatAdminOnly      = true;
	$Pref::Server::ND::SaveAdminOnly       = true;
	$Pref::Server::ND::LoadAdminOnly       = false;
	$Pref::Server::ND::FillBricksAdminOnly = true;

	//Settings
	$Pref::Server::ND::TrustLimit          = 2;
	$Pref::Server::ND::AdminTrustBypass1   = true;
	$Pref::Server::ND::AdminTrustBypass2   = false;
	$Pref::Server::ND::SelectPublicBricks  = true;

	$Pref::Server::ND::MaxBricksAdmin      = 1000000;
	$Pref::Server::ND::MaxBricksPlayer     = 10000;
	$Pref::Server::ND::MaxBoxSizeAdmin     = 1024;
	$Pref::Server::ND::MaxBoxSizePlayer    = 64;

	$Pref::Server::ND::SelectTimeoutMS     = 400;
	$Pref::Server::ND::PlantTimeoutMS      = 400;

	//Advanced
	$Pref::Server::ND::PlayMenuSounds      = true;
	$Pref::Server::ND::MaxGhostBricks      = 1500;
	$Pref::Server::ND::InstantGhostBricks  = 150;
	$Pref::Server::ND::ScatterGhostBricks  = true;
	$Pref::Server::ND::ProcessPerTick      = 300;
	$Pref::Server::ND::BoxSelectChunkDim   = 6;
	$Pref::Server::ND::SymTableOnStart     = false;

	//Always set this to false so we don't accidently reset the prefs
	$ND::RestoreDefaultPrefs = false;
}

//Erases outdated prefs from the config file
function ndDeleteOutdatedPrefs()
{
	//Step 1: Copy all current prefs
	//Limits
	%adminOnly           = $Pref::Server::ND::AdminOnly;
	%paintAdminOnly      = $Pref::Server::ND::PaintAdminOnly;
	%paintFxAdminOnly    = $Pref::Server::ND::PaintFxAdminOnly;
	%wrenchAdminOnly     = $Pref::Server::ND::WrenchAdminOnly;
	%floatAdminOnly      = $Pref::Server::ND::FloatAdminOnly;
	%saveAdminOnly       = $Pref::Server::ND::SaveAdminOnly;
	%loadAdminOnly       = $Pref::Server::ND::LoadAdminOnly;
	%fillBricksAdminOnly = $Pref::Server::ND::FillBricksAdminOnly;
	//Settings
	%trustLimit          = $Pref::Server::ND::TrustLimit;
	%adminTrustBypass1   = $Pref::Server::ND::AdminTrustBypass1;
	%adminTrustBypass2   = $Pref::Server::ND::AdminTrustBypass2;
	%selectPublicBricks  = $Pref::Server::ND::SelectPublicBricks;
	%maxBricksAdmin      = $Pref::Server::ND::MaxBricksAdmin;
	%maxBricksPlayer     = $Pref::Server::ND::MaxBricksPlayer;
	%maxBoxSizeAdmin     = $Pref::Server::ND::MaxBoxSizeAdmin;
	%maxBoxSizePlayer    = $Pref::Server::ND::MaxBoxSizePlayer;
	%selectTimeoutMS     = $Pref::Server::ND::SelectTimeoutMS;
	%plantTimeoutMS      = $Pref::Server::ND::PlantTimeoutMS;
	//Advanced
	%playMenuSounds      = $Pref::Server::ND::PlayMenuSounds;
	%maxGhostBricks      = $Pref::Server::ND::MaxGhostBricks;
	%instantGhostBricks  = $Pref::Server::ND::InstantGhostBricks;
	%scatterGhostBricks  = $Pref::Server::ND::ScatterGhostBricks;
	%processPerTick      = $Pref::Server::ND::ProcessPerTick;
	%boxSelectChunkDim   = $Pref::Server::ND::BoxSelectChunkDim;
	%symTableOnStart     = $Pref::Server::ND::SymTableOnStart;

	//Step 2: Delete everything
	deleteVariables("$Pref::Server::ND::*");

	//Step 3: Set current prefs again
	//Limits
	$Pref::Server::ND::AdminOnly           = %adminOnly;
	$Pref::Server::ND::PaintAdminOnly      = %paintAdminOnly;
	$Pref::Server::ND::PaintFxAdminOnly    = %paintFxAdminOnly;
	$Pref::Server::ND::WrenchAdminOnly     = %wrenchAdminOnly;
	$Pref::Server::ND::FloatAdminOnly      = %floatAdminOnly;
	$Pref::Server::ND::SaveAdminOnly       = %saveAdminOnly;
	$Pref::Server::ND::LoadAdminOnly       = %loadAdminOnly;
	$Pref::Server::ND::FillBricksAdminOnly = %fillBricksAdminOnl;
	//Settings
	$Pref::Server::ND::TrustLimit          = %trustLimit;
	$Pref::Server::ND::AdminTrustBypass1   = %adminTrustBypass1;
	$Pref::Server::ND::AdminTrustBypass2   = %adminTrustBypass2;
	$Pref::Server::ND::SelectPublicBricks  = %selectPublicBricks;
	$Pref::Server::ND::MaxBricksAdmin      = %maxBricksAdmin;
	$Pref::Server::ND::MaxBricksPlayer     = %maxBricksPlayer;
	$Pref::Server::ND::MaxBoxSizeAdmin     = %maxBoxSizeAdmin;
	$Pref::Server::ND::MaxBoxSizePlayer    = %maxBoxSizePlayer;
	$Pref::Server::ND::SelectTimeoutMS     = %selectTimeoutMS;
	$Pref::Server::ND::PlantTimeoutMS      = %plantTimeoutMS;
	//Advanced
	$Pref::Server::ND::PlayMenuSounds      = %playMenuSounds;
	$Pref::Server::ND::MaxGhostBricks      = %maxGhostBricks;
	$Pref::Server::ND::InstantGhostBricks  = %instantGhostBricks;
	$Pref::Server::ND::ScatterGhostBricks  = %scatterGhostBricks;
	$Pref::Server::ND::ProcessPerTick      = %processPerTick;
	$Pref::Server::ND::BoxSelectChunkDim   = %boxSelectChunkDim;
	$Pref::Server::ND::SymTableOnStart     = %symTableOnStart;
}
