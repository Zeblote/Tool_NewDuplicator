// Resizeable box to select all bricks inside a volume.
// -------------------------------------------------------------------

//Create a new selection box
function ND_SelectionBox(%shapeName)
{
	ND_ServerGroup.add(
		%this = new ScriptObject(ND_SelectionBox)
	);

	%this.innerBox = new StaticShape(){datablock = ND_SelectionBoxInner;};
	%this.outerBox = new StaticShape(){datablock = ND_SelectionBoxOuter;};
	%this.shapeName = new StaticShape(){datablock = ND_SelectionBoxShapeName;};

	%this.corner1 = new StaticShape(){datablock = ND_SelectionBoxOuter;};
	%this.corner2 = new StaticShape(){datablock = ND_SelectionBoxOuter;};
	%this.selectedCorner = true;

	%this.innerBox.setScopeAlways();
	%this.outerBox.setScopeAlways();
	%this.shapeName.setScopeAlways();
	%this.corner1.setScopeAlways();
	%this.corner2.setScopeAlways();

	for(%i = 0; %i < 4; %i++)
	{
		%this.border_x[%i] = new StaticShape(){datablock = ND_SelectionBoxBorder;};
		%this.border_y[%i] = new StaticShape(){datablock = ND_SelectionBoxBorder;};
		%this.border_z[%i] = new StaticShape(){datablock = ND_SelectionBoxBorder;};

		%this.border_x[%i].setScopeAlways();
		%this.border_y[%i].setScopeAlways();
		%this.border_z[%i].setScopeAlways();
	}

	%this.boxName = %shapeName;
	%this.setNormalMode();

	return %this;
}

//Destroy static shapes when selection box is removed
function ND_SelectionBox::onRemove(%this)
{
	%this.innerBox.delete();
	%this.outerBox.delete();
	%this.shapeName.delete();

	%this.corner1.delete();
	%this.corner2.delete();

	for(%i = 0; %i < 4; %i++)
	{
		%this.border_x[%i].delete();
		%this.border_y[%i].delete();
		%this.border_z[%i].delete();
	}
}

//Set normal color values and borders
function ND_SelectionBox::setNormalMode(%this)
{
	%this.innerColor  = "0 0 0 0.60";
	%this.outerColor  = "0 0 0 0.35";

	%this.borderColor = "1 0.84 0 0.99";
	%this.borderColorSelected = "0 0 1 0.99";

	%this.cornerColor1 = "0.8 0.74 0 0.99";
	%this.cornerColor2 = "1 0.94 0.1 0.99";

	%this.cornerColorSelected1 = "0 0.2 1 0.99";
	%this.cornerColorSelected2 = "0 0.1 0.9 0.99";

	%this.isNormalMode = true;

	//Unhide the corners and inner/outer box (hidden in disabled mode)
	%this.innerBox.unHideNode("ALL");
	%this.corner1.unHideNode("ALL");
	%this.corner2.unHideNode("ALL");

	if(%this.hasVolume())
		%this.outerBox.unHideNode("ALL");

	//Apply changes
	%this.applyColors();
	%this.setSize(%this.point1, %this.point2);
	%this.shapeName.setShapeName(%this.boxName);
}

//Set grayscale color values and slightly smaller border
function ND_SelectionBox::setDisabledMode(%this)
{
	%this.borderColor = "0.1 0.1 0.1 0.4";
	%this.borderColorSelected = "0.1 0.1 0.1 0.4";

	%this.isNormalMode = false;

	//Hide the corners and inner/outer box (looks better)
	%this.innerBox.hideNode("ALL");
	%this.outerBox.hideNode("ALL");
	%this.corner1.hideNode("ALL");
	%this.corner2.hideNode("ALL");

	//Apply changes
	%this.applyColors();
	%this.setSize(%this.point1, %this.point2);
	%this.shapeName.setShapeName("");
}

//Apply color changes to the selection box
function ND_SelectionBox::applyColors(%this)
{
	%this.innerBox.setNodeColor("ALL", %this.innerColor);
	%this.outerBox.setNodeColor("ALL", %this.outerColor);

	%this.shapeName.setShapeNameColor(%this.borderColor);

	for(%i = 0; %i < 4; %i++)
	{
		%this.border_x[%i].setNodeColor("ALL", %this.borderColor);
		%this.border_y[%i].setNodeColor("ALL", %this.borderColor);
		%this.border_z[%i].setNodeColor("ALL", %this.borderColor);
	}

	%bColor = %this.borderColorSelected;

	if(%this.selectedCorner)
	{
		%this.border_x2.setNodeColor("ALL", %bColor);
		%this.border_y2.setNodeColor("ALL", %bColor);
		%this.border_z2.setNodeColor("ALL", %bColor);

		%corner1 = %this.corner1;
		%corner2 = %this.corner2;
	}
	else
	{
		%this.border_x0.setNodeColor("ALL", %bColor);
		%this.border_y0.setNodeColor("ALL", %bColor);
		%this.border_z0.setNodeColor("ALL", %bColor);

		%corner1 = %this.corner2;
		%corner2 = %this.corner1;
	}

	%corner1.setNodeColor("out+X", %this.borderColor);
	%corner1.setNodeColor("out-X", %this.borderColor);
	%corner1.setNodeColor("out+Y", %this.cornerColor1);
	%corner1.setNodeColor("out-Y", %this.cornerColor1);
	%corner1.setNodeColor("out+Z", %this.cornerColor2);
	%corner1.setNodeColor("out-Z", %this.cornerColor2);

	//Illusion of shaded box
	%corner2.setNodeColor("out+X", %this.borderColorSelected);
	%corner2.setNodeColor("out-X", %this.borderColorSelected);
	%corner2.setNodeColor("out+Y", %this.cornerColorSelected1);
	%corner2.setNodeColor("out-Y", %this.cornerColorSelected1);
	%corner2.setNodeColor("out+Z", %this.cornerColorSelected2);
	%corner2.setNodeColor("out-Z", %this.cornerColorSelected2);
}

//Return current size of selection box
function ND_SelectionBox::getSize(%this)
{
	%x1 = getWord(%this.point1, 0);
	%y1 = getWord(%this.point1, 1);
	%z1 = getWord(%this.point1, 2);

	%x2 = getWord(%this.point2, 0);
	%y2 = getWord(%this.point2, 1);
	%z2 = getWord(%this.point2, 2);

	%min = getMin(%x1, %x2) SPC getMin(%y1, %y2) SPC getMin(%z1, %z2);
	%max = getMax(%x1, %x2) SPC getMax(%y1, %y2) SPC getMax(%z1, %z2);

	return vectorSub(%max, %min);
}

//Return current world box of selection box
function ND_SelectionBox::getWorldBox(%this)
{
	%x1 = getWord(%this.point1, 0);
	%y1 = getWord(%this.point1, 1);
	%z1 = getWord(%this.point1, 2);

	%x2 = getWord(%this.point2, 0);
	%y2 = getWord(%this.point2, 1);
	%z2 = getWord(%this.point2, 2);

	%min = getMin(%x1, %x2) SPC getMin(%y1, %y2) SPC getMin(%z1, %z2);
	%max = getMax(%x1, %x2) SPC getMax(%y1, %y2) SPC getMax(%z1, %z2);

	return %min SPC %max;
}

//Resize the selection box
function ND_SelectionBox::setSize(%this, %point1, %point2)
{
	if(getWordCount(%point1) == 6)
	{
		%point2 = getWords(%point1, 3, 5);
		%point1 = getWords(%point1, 0, 2);
	}

	%this.point1 = %point1;
	%this.point2 = %point2;

	%x1 = getWord(%point1, 0);
	%y1 = getWord(%point1, 1);
	%z1 = getWord(%point1, 2);

	%x2 = getWord(%point2, 0);
	%y2 = getWord(%point2, 1);
	%z2 = getWord(%point2, 2);

	%len_x = mAbs(%x2 - %x1);
	%len_y = mAbs(%y2 - %y1);
	%len_z = mAbs(%z2 - %z1);

	%center_x = (%x1 + %x2) / 2;
	%center_y = (%y1 + %y2) / 2;
	%center_z = (%z1 + %z2) / 2;

	%rot_x = "0 1 0 1.57079";
	%rot_y = "1 0 0 1.57079";
	%rot_z = "0 0 1 0";

	%this.innerBox.setTransform(%center_x SPC %center_y SPC %center_z);
	%this.outerBox.setTransform(%center_x SPC %center_y SPC %center_z);
	%this.shapeName.setTransform(%center_X SPC %center_y SPC %z2);

	%this.border_x0.setTransform(%center_x SPC %y1 SPC %z1 SPC %rot_x);
	%this.border_x1.setTransform(%center_x SPC %y2 SPC %z1 SPC %rot_x);
	%this.border_x2.setTransform(%center_x SPC %y2 SPC %z2 SPC %rot_x);
	%this.border_x3.setTransform(%center_x SPC %y1 SPC %z2 SPC %rot_x);

	%this.border_y0.setTransform(%x1 SPC %center_y SPC %z1 SPC %rot_y);
	%this.border_y1.setTransform(%x2 SPC %center_y SPC %z1 SPC %rot_y);
	%this.border_y2.setTransform(%x2 SPC %center_y SPC %z2 SPC %rot_y);
	%this.border_y3.setTransform(%x1 SPC %center_y SPC %z2 SPC %rot_y);

	%this.border_z0.setTransform(%x1 SPC %y1 SPC %center_z SPC %rot_z);
	%this.border_z1.setTransform(%x2 SPC %y1 SPC %center_z SPC %rot_z);
	%this.border_z2.setTransform(%x2 SPC %y2 SPC %center_z SPC %rot_z);
	%this.border_z3.setTransform(%x1 SPC %y2 SPC %center_z SPC %rot_z);

	%this.corner1.setTransform(%x1 SPC %y1 SPC %z1);
	%this.corner2.setTransform(%x2 SPC %y2 SPC %z2);

	%this.innerBox.setScale(%len_x - 0.02 SPC %len_y - 0.02 SPC %len_z - 0.02);
	%this.outerBox.setScale(%len_x + 0.02 SPC %len_y + 0.02 SPC %len_z + 0.02);

	if(%this.isNormalMode)
	{
		//Normal mode (box with two colored corners)
		%maxLen = getMax(getMax(%len_x, %len_y), %len_z);
		%width = (7/1024) * %maxLen + 1;

		for(%i = 0; %i < 4; %i++)
		{
			%this.border_x[%i].setScale(%width SPC %width SPC %len_x + %width * 0.05);
			%this.border_y[%i].setScale(%width SPC %width SPC %len_y + %width * 0.05);
			%this.border_z[%i].setScale(%width SPC %width SPC %len_z + %width * 0.05);
		}

		if(%this.selectedCorner)
		{
			%width1 = %width;
			%width2 = %width + 0.02;
		}
		else
		{
			%width1 = %width + 0.02;
			%width2 = %width;
		}

		//The borders touching the two corners are thicker to prevent Z fighting
		//with the highlight box if it covers the same area as the selection
		%this.border_x0.setScale(%width1 SPC %width1 SPC %len_x - %width * 0.05);
		%this.border_y0.setScale(%width1 SPC %width1 SPC %len_y - %width * 0.05);
		%this.border_z0.setScale(%width1 SPC %width1 SPC %len_z - %width * 0.05);

		%this.border_x2.setScale(%width2 SPC %width2 SPC %len_x - %width * 0.05);
		%this.border_y2.setScale(%width2 SPC %width2 SPC %len_y - %width * 0.05);
		%this.border_z2.setScale(%width2 SPC %width2 SPC %len_z - %width * 0.05);

		//Corners scale with the border width
		%cs1 = 0.35 * %width1;
		%cs2 = 0.35 * %width2;

		%this.corner1.setScale(%cs1 SPC %cs1 SPC %cs1);
		%this.corner2.setScale(%cs2 SPC %cs2 SPC %cs2);
	}
	else
	{
		//Disabled mode (transparent greyscale box)
		%maxLen = getMax(getMax(%len_x, %len_y), %len_z);
		%width = (21/5120) * %maxLen + 1;

		for(%i = 0; %i < 4; %i++)
		{
			//Horizontal borders are a bit shorter to prevent z fighting
			%this.border_x[%i].setScale(%width SPC %width SPC %len_x - %width * 0.05);
			%this.border_y[%i].setScale(%width SPC %width SPC %len_y - %width * 0.05);

			%this.border_z[%i].setScale(%width SPC %width SPC %len_z + %width * 0.05);
		}
	}
}

//Resize the selection box and align it to a player
function ND_SelectionBox::setSizeAligned(%this, %point1, %point2, %player)
{
	//Set the selection box to correct orientation
	%x1 = getWord(%point1, 0);
	%y1 = getWord(%point1, 1);
	%z1 = getWord(%point1, 2);

	%x2 = getWord(%point2, 0);
	%y2 = getWord(%point2, 1);
	%z2 = getWord(%point2, 2);

	switch(getAngleIDFromPlayer(%player))
	{
		case 0:
			%p1 = %x1 SPC %y2 SPC %z1;
			%p2 = %x2 SPC %y1 SPC %z2;

		case 1:
			%p1 = %x1 SPC %y1 SPC %z1;
			%p2 = %x2 SPC %y2 SPC %z2;

		case 2:
			%p1 = %x2 SPC %y1 SPC %z1;
			%p2 = %x1 SPC %y2 SPC %z2;

		case 3:
			%p1 = %x2 SPC %y2 SPC %z1;
			%p2 = %x1 SPC %y1 SPC %z2;
	}

	//Select first corner
	if(!%this.selectedCorner)
	{
		%this.selectedCorner = true;
		%this.applyColors();
	}

	%this.setSize(%p1, %p2);
}

//Select one of the two corners
function ND_SelectionBox::switchCorner(%this)
{
	%this.selectedCorner = !%this.selectedCorner;

	if(%this.selectedCorner)
		serverPlay3d(BrickRotateSound, %this.point2);
	else
		serverPlay3d(BrickRotateSound, %this.point1);

	%this.setSize(%this.point1, %this.point2);
	%this.applyColors();
}

//Move the selected corner
function ND_SelectionBox::shiftCorner(%this, %offset, %limit)
{
	%oldP1 = %this.point1;
	%oldP2 = %this.point2;
	%limitReached = false;

	//Size of a plate in TU
	%unit[0] = 0.5;
	%unit[1] = 0.5;
	%unit[2] = 0.2;

	for(%dim = 0; %dim < 3; %dim++)
	{
		//Copy current
		%point1[%dim] = getWord(%this.point1, %dim);
		%point2[%dim] = getWord(%this.point2, %dim);

		//Get the size of the box in the current axis after resizing
		%size = getWord(%this.point2, %dim) - getWord(%this.point1, %dim);

		if(%this.selectedCorner)
		{
			//Update point2
			%size += getWord(%offset, %dim);
			%point2[%dim] += getWord(%offset, %dim);

			//Check limits
			if(mAbs(%size) > %limit)
			{
				%limitReached = true;
				%point2[%dim] -= %size - %limit * (mAbs(%size) / %size);
			}
		}
		else
		{
			//Update point1
			%size -= getWord(%offset, %dim);
			%point1[%dim] += getWord(%offset, %dim);

			//Check limits
			if(mAbs(%size) > %limit)
			{
				%limitReached = true;
				%point1[%dim] += %size - %limit * (mAbs(%size) / %size);
			}
		}
	}

	//Update corner positions
	%point1 = %point1[0] SPC %point1[1] SPC %point1[2];
	%point2 = %point2[0] SPC %point2[1] SPC %point2[2];
	%this.setSize(%point1, %point2);

	//Play sounds
	if(%this.selectedCorner)
		%soundPoint = %point2;
	else
		%soundPoint = %point1;

	if(%point1 !$= %oldP1 || %point2 !$= %oldP2)
		serverPlay3d(BrickMoveSound, %soundPoint);
	else
		serverPlay3d(errorSound, %soundPoint);

	//Hide outer box on selection boxes without volume
	if(%this.hasVolume())
		%this.outerBox.unHideNode("ALL");
	else
		%this.outerBox.hideNode("ALL");

	return %limitReached;
}

//Move the entire box
function ND_SelectionBox::shift(%this, %offset)
{
	%this.point1 = vectorAdd(%this.point1, %offset);
	%this.point2 = vectorAdd(%this.point2, %offset);

	%this.setSize(%this.point1, %this.point2);

	//Play sounds
	if(%this.selectedCorner)
		serverPlay3d(BrickMoveSound, %this.point1);
	else
		serverPlay3d(BrickMoveSound, %this.point2);
}

//Rotate the entire box
function ND_SelectionBox::rotate(%this, %direction)
{
	%point1 = %this.point1;
	%point2 = %this.point2;
	%center = vectorScale(vectorAdd(%point1, %point2), 0.5);

	%brickSizeX = mAbs(mFloatLength((getWord(%point2, 0) - getWord(%point1, 0)) * 2, 0));
	%brickSizeY = mAbs(mFloatLength((getWord(%point2, 1) - getWord(%point1, 1)) * 2, 0));

	%shiftCorrect = "0 0 0";

	if((%brickSizeX % 2) != (%brickSizeY % 2))
	{
		if(%brickSizeX % 2)
			%shiftCorrect = "-0.25 -0.25 0";
		else
			%shiftCorrect = "0.25 0.25 0";
	}

	%point1 = vectorAdd(ndRotateVector(vectorSub(%point1, %center), %direction), %center);
	%point2 = vectorAdd(ndRotateVector(vectorSub(%point2, %center), %direction), %center);
	%this.setSize(vectorAdd(%point1, %shiftCorrect), vectorAdd(%point2, %shiftCorrect));

	//Play sounds
	if(%this.selectedCorner)
		serverPlay3d(BrickRotateSound, %this.point1);
	else
		serverPlay3d(BrickRotateSound, %this.point2);
}

//Check if the box has a volume
function ND_SelectionBox::hasVolume(%this)
{
	if(mAbs(getWord(%this.point1, 0) - getWord(%this.point2, 0)) < 0.05
	|| mAbs(getWord(%this.point1, 1) - getWord(%this.point2, 1)) < 0.05
	|| mAbs(getWord(%this.point1, 2) - getWord(%this.point2, 2)) < 0.05)
		return false;

	return true;
}
