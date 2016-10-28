// Manually sets up symmetry planes for certain bricks with bad geometry.
// -------------------------------------------------------------------

//Manual symmetry can be set using the following variables:
// $ND::ManualSymmetry[UIName] = {0 - 5}
// $ND::ManualSymmetryDB[UIName] = Other UIName
// $ND::ManualSymmetryOffset[UIName] = {0 - 3}

// $ND::ManualSymmetryZ[UIName] = {true, false}
// $ND::ManualSymmetryZDB[UIName] = Other UIName
// $ND::ManualSymmetryZOffset[UIName] = {0 - 3}

//Built-in Bricks
$ND::ManualSymmetryZ["1x1 Round"] = true;
$ND::ManualSymmetryZ["1x1F Round"] = true;
$ND::ManualSymmetryZ["Castle Wall"] = true;
$ND::ManualSymmetryZ["1x4x5 Window"] = true;

//Brick_V15
$ND::ManualSymmetry["1x4x2 Bars"] = 1;
$ND::ManualSymmetryZ["1x4x2 Bars"] = true;

//Brick_Treasure_Chest
$ND::ManualSymmetry["Treasure Chest"] = 2;

//Brick_Teledoor
$ND::ManualSymmetryZ["Teledoor"] = 1;

//Brick_Halloween
$ND::ManualSymmetry["Skull Cool Open"] = 2;
$ND::ManualSymmetry["Skull Cool"] = 2;

$ND::ManualSymmetry["Pumpkin"] = 3;
$ND::ManualSymmetry["Pumpkin_Face"] = 3;
$ND::ManualSymmetry["Pumpkin_Scared"] = 3;
$ND::ManualSymmetry["Pumpkin_Ascii"] = 3;

//Brick_PoleAdapters
$ND::ManualSymmetry["1x1x3 Pole"] = 1;
$ND::ManualSymmetry["1x1 Pole"] = 1;
$ND::ManualSymmetry["1x1F Pole"] = 1;

$ND::ManualSymmetry["1x1F Pole Plus"] = 2;
$ND::ManualSymmetry["1x1F Pole Corner"] = 5;
$ND::ManualSymmetry["1x1F Pole Corner up"] = 2;
$ND::ManualSymmetry["1x1F Pole Corner down"] = 2;
$ND::ManualSymmetry["1x1F Pole T"] = 5;
$ND::ManualSymmetry["1x1F Pole T up"] = 2;
$ND::ManualSymmetry["1x1F Pole T down"] = 2;
$ND::ManualSymmetry["1x1F Pole X Vert"] = 2;
$ND::ManualSymmetry["1x1F Pole X"] = 1;

$ND::ManualSymmetryZ["1x1F Pole Plus"] = true;
$ND::ManualSymmetryZ["1x1F Pole Corner"] = true;
$ND::ManualSymmetryZ["1x1F Pole Corner up"] = false;
$ND::ManualSymmetryZ["1x1F Pole Corner down"] = false;
$ND::ManualSymmetryZ["1x1F Pole T"] = true;
$ND::ManualSymmetryZ["1x1F Pole T up"] = false;
$ND::ManualSymmetryZ["1x1F Pole T down"] = false;
$ND::ManualSymmetryZ["1x1F Pole X Vert"] = true;
$ND::ManualSymmetryZ["1x1F Pole X"] = true;

$ND::ManualSymmetryZDB["1x1F Pole Corner up"] = "1x1F Pole Corner down";
$ND::ManualSymmetryZDB["1x1F Pole Corner down"] = "1x1F Pole Corner up";
$ND::ManualSymmetryZDB["1x1F Pole T up"] = "1x1F Pole T down";
$ND::ManualSymmetryZDB["1x1F Pole T down"] = "1x1F Pole T up";

$ND::ManualSymmetryZOffset["1x1F Pole Corner up"] = 0;
$ND::ManualSymmetryZOffset["1x1F Pole Corner down"] = 0;
$ND::ManualSymmetryZOffset["1x1F Pole T up"] = 0;
$ND::ManualSymmetryZOffset["1x1F Pole T down"] = 0;

//Brick_PoleDiagonals
$ND::ManualSymmetryZ["1x1f Horiz. Diag."] = true;
$ND::ManualSymmetryZ["2x2f Horiz. Diag."] = true;
$ND::ManualSymmetryZ["3x3f Horiz. Diag."] = true;
$ND::ManualSymmetryZ["4x4f Horiz. Diag."] = true;
$ND::ManualSymmetryZ["5x5f Horiz. Diag."] = true;
$ND::ManualSymmetryZ["6x6f Horiz. Diag."] = true;

$ND::ManualSymmetry["1x1 Vert. Diag. A"] = 2;
$ND::ManualSymmetry["2x2 Vert. Diag. A"] = 2;
$ND::ManualSymmetry["3x3 Vert. Diag. A"] = 2;
$ND::ManualSymmetry["4x4 Vert. Diag. A"] = 2;
$ND::ManualSymmetry["5x5 Vert. Diag. A"] = 2;
$ND::ManualSymmetry["6x6 Vert. Diag. A"] = 2;
$ND::ManualSymmetry["1x1 Vert. Diag. B"] = 2;
$ND::ManualSymmetry["2x2 Vert. Diag. B"] = 2;
$ND::ManualSymmetry["3x3 Vert. Diag. B"] = 2;
$ND::ManualSymmetry["4x4 Vert. Diag. B"] = 2;
$ND::ManualSymmetry["5x5 Vert. Diag. B"] = 2;
$ND::ManualSymmetry["6x6 Vert. Diag. B"] = 2;

$ND::ManualSymmetryZ["1x1 Vert. Diag. A"] = false;
$ND::ManualSymmetryZ["2x2 Vert. Diag. A"] = false;
$ND::ManualSymmetryZ["3x3 Vert. Diag. A"] = false;
$ND::ManualSymmetryZ["4x4 Vert. Diag. A"] = false;
$ND::ManualSymmetryZ["5x5 Vert. Diag. A"] = false;
$ND::ManualSymmetryZ["6x6 Vert. Diag. A"] = false;
$ND::ManualSymmetryZ["1x1 Vert. Diag. B"] = false;
$ND::ManualSymmetryZ["2x2 Vert. Diag. B"] = false;
$ND::ManualSymmetryZ["3x3 Vert. Diag. B"] = false;
$ND::ManualSymmetryZ["4x4 Vert. Diag. B"] = false;
$ND::ManualSymmetryZ["5x5 Vert. Diag. B"] = false;
$ND::ManualSymmetryZ["6x6 Vert. Diag. B"] = false;

$ND::ManualSymmetryZDB["1x1 Vert. Diag. A"] = "1x1 Vert. Diag. B";
$ND::ManualSymmetryZDB["2x2 Vert. Diag. A"] = "2x2 Vert. Diag. B";
$ND::ManualSymmetryZDB["3x3 Vert. Diag. A"] = "3x3 Vert. Diag. B";
$ND::ManualSymmetryZDB["4x4 Vert. Diag. A"] = "4x4 Vert. Diag. B";
$ND::ManualSymmetryZDB["5x5 Vert. Diag. A"] = "5x5 Vert. Diag. B";
$ND::ManualSymmetryZDB["6x6 Vert. Diag. A"] = "6x6 Vert. Diag. B";
$ND::ManualSymmetryZDB["1x1 Vert. Diag. B"] = "1x1 Vert. Diag. A";
$ND::ManualSymmetryZDB["2x2 Vert. Diag. B"] = "2x2 Vert. Diag. A";
$ND::ManualSymmetryZDB["3x3 Vert. Diag. B"] = "3x3 Vert. Diag. A";
$ND::ManualSymmetryZDB["4x4 Vert. Diag. B"] = "4x4 Vert. Diag. A";
$ND::ManualSymmetryZDB["5x5 Vert. Diag. B"] = "5x5 Vert. Diag. A";
$ND::ManualSymmetryZDB["6x6 Vert. Diag. B"] = "6x6 Vert. Diag. A";

$ND::ManualSymmetryZOffset["1x1 Vert. Diag. A"] = 2;
$ND::ManualSymmetryZOffset["2x2 Vert. Diag. A"] = 2;
$ND::ManualSymmetryZOffset["3x3 Vert. Diag. A"] = 2;
$ND::ManualSymmetryZOffset["4x4 Vert. Diag. A"] = 2;
$ND::ManualSymmetryZOffset["5x5 Vert. Diag. A"] = 2;
$ND::ManualSymmetryZOffset["6x6 Vert. Diag. A"] = 2;
$ND::ManualSymmetryZOffset["1x1 Vert. Diag. B"] = 2;
$ND::ManualSymmetryZOffset["2x2 Vert. Diag. B"] = 2;
$ND::ManualSymmetryZOffset["3x3 Vert. Diag. B"] = 2;
$ND::ManualSymmetryZOffset["4x4 Vert. Diag. B"] = 2;
$ND::ManualSymmetryZOffset["5x5 Vert. Diag. B"] = 2;
$ND::ManualSymmetryZOffset["6x6 Vert. Diag. B"] = 2;

//Brick_GrillPlate
$ND::ManualSymmetry["Grill Corner"] = 4;

//Brick_Bevel
$ND::ManualSymmetry["1x1F Beveled"] = 1;
$ND::ManualSymmetry["1x1FF Beveled"] = 1;
$ND::ManualSymmetry["1x2F Beveled"] = 1;
$ND::ManualSymmetry["1x2FF Beveled"] = 1;
$ND::ManualSymmetry["1x1 Beveled"] = 1;
$ND::ManualSymmetry["1x1x2 Beveled"] = 1;
$ND::ManualSymmetry["1x2 Beveled"] = 1;
$ND::ManualSymmetry["2x2F Beveled"] = 1;
$ND::ManualSymmetry["2x4F Beveled"] = 1;
$ND::ManualSymmetry["2x4FF Beveled"] = 1;

$ND::ManualSymmetryZ["1x1F Beveled"] = true;
$ND::ManualSymmetryZ["1x1FF Beveled"] = true;
$ND::ManualSymmetryZ["1x2F Beveled"] = true;
$ND::ManualSymmetryZ["1x2FF Beveled"] = true;
$ND::ManualSymmetryZ["1x1 Beveled"] = true;
$ND::ManualSymmetryZ["1x1x2 Beveled"] = true;
$ND::ManualSymmetryZ["1x2 Beveled"] = true;
$ND::ManualSymmetryZ["2x2F Beveled"] = true;
$ND::ManualSymmetryZ["2x4F Beveled"] = true;
$ND::ManualSymmetryZ["2x4FF Beveled"] = true;

//Brick_1RandomPack
$ND::ManualSymmetry["2x2x2 Octo Elbow Horz"] = 4;

$ND::ManualSymmetryZ["1x1 Octo"] = true;
$ND::ManualSymmetryZ["1x1x2 Octo"] = true;
$ND::ManualSymmetryZ["2x2x2 Octo T Horz"] = true;
$ND::ManualSymmetryZ["2x2x2 Octo Elbow Horz"] = true;

$ND::ManualSymmetryZ["2x3x2 Octo Offset"] = false;
$ND::ManualSymmetryZDB["2x3x2 Octo Offset"] = "2x3x2 Octo Offset";
$ND::ManualSymmetryZOffset["2x3x2 Octo Offset"] = 2;

//Brick_Fence
$ND::ManualSymmetry["1x4 Fence"] = 3;

//Brick_SmallBricks
$ND::ManualSymmetry["0.25x0.25 Corner"] = 4;
$ND::ManualSymmetry["0.25x0.25F Corner"] = 4;
$ND::ManualSymmetry["0.5x0.5 Corner"] = 4;
$ND::ManualSymmetry["0.5x0.5F Corner"] = 4;
$ND::ManualSymmetry["0.75x0.75 Corner"] = 4;
$ND::ManualSymmetry["0.75x0.75F Corner"] = 4;
