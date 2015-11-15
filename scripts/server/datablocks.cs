// * ######################################################################
// *
// *    New Duplicator - Scripts - Server
// *    Datablocks
// *
// *    -------------------------------------------------------------------
// *    Create datablocks required for the new duplicator
// *
// * ######################################################################

//Duplicator to hold in your hand
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

//Duplicator Item
datablock ItemData(ND_DupliItem)
{
	cameraMaxDist   = 0.1;
	canDrop         = 1;
	category        = "Weapon";
	className       = "Tool";
	colorShiftColor = "1 0.84 0 1";
	density         = 0.2;
	doColorShift    = 1;
	elasticity      = 0.2;
	emap            = 1;
	friction        = 0.6;
	iconName        = "base/client/ui/itemIcons/wand";
	image           = "ND_DupliImage";
	shapeFile       = "base/data/shapes/wand.dts";
	uiName          = "Duplicator";
};

//Particles for explosion
datablock ParticleData(ND_DupliExplosionParticle)
{
	colors[0]          = "1 0.84 0 0.9";
	colors[1]          = "1 0.84 0 0.7";
	colors[2]          = "1 0.84 0 0.5";
	gravityCoefficient = 0;
	lifetimeMS         = 400;
	lifetimeVarianceMS = 200;
	sizes[0]           = 0.6;
	sizes[1]           = 0.4;
	sizes[2]           = 0.3;
	spinRandomMax      = 90;
	spinRandomMin      = -90;
	textureName        = "base/data/particles/ring";
	times[1]           = 0.8;
	times[2]           = 1;
};

//Emitter for explosion
datablock ParticleEmitterData(ND_DupliExplosionEmitter)
{
	ejectionOffset   = 0.5;
	ejectionPeriodMS = 4;
	ejectionVelocity = 3;
	particles        = ND_DupliExplosionParticle;
	periodVarianceMS = 2;
	thetaMax         = 180;
	velocityVariance = 0;
};

//Explosion 
datablock ExplosionData(ND_DupliExplosion)
{
	camShakeDuration = 0.5;
	camShakeFreq     = "1 1 1";
	emitter[0]       = ND_DupliExplosionEmitter;
	faceViewer       = 1;
	lifetimeMS       = 180;
	lightEndRadius   = 0;
	lightStartColor  = "0 0 0 0";
	lightStartRadius = 0;
	shakeCamera      = 1;
	soundProfile     = "wandHitSound";
};

//Projectile to make explosion
datablock ProjectileData(ND_DupliProjectile)
{
	bounceElasticity = 0;
	bounceFriction   = 0;
	explodeOnDeath   = 1;
	explosion        = ND_DupliExplosion;
	fadeDelay        = 2;
	gravityMod       = 0;
	lifetime         = 0;
	range            = 10;
};

//Idle particles
datablock ParticleData(ND_DupliParticleA)
{
	colors[0]          = "1 0.84 0 0.9";
	colors[1]          = "1 0.84 0 0.7";
	colors[2]          = "1 0.84 0 0.5";
	gravityCoefficient = -0.5;
	lifetimeMS         = 400;
	lifetimeVarianceMS = 200;
	sizes[0]           = 0.1;
	sizes[1]           = 0.4;
	sizes[2]           = 0.6;
	spinRandomMax      = 90;
	spinRandomMin      = -90;
	textureName        = "base/data/particles/ring";
	times[1]           = 0.8;
	times[2]           = 1;
};

//Idle emitter
datablock ParticleEmitterData(ND_DupliEmitterA)
{
	ejectionOffset   = 0.09;
	ejectionPeriodMS = 50;
	ejectionVelocity = 0.2;
	particles        = ND_DupliParticleA;
	periodVarianceMS = 2;
	thetaMax         = 180;
	velocityVariance = 0;
};

//Active particles
datablock ParticleData(ND_DupliParticleB)
{
	colors[0]          = "1 0.84 0 0.9";
	colors[1]          = "1 0.84 0 0.7";
	colors[2]          = "1 0.84 0 0.5";
	gravityCoefficient = -0.4;
	dragCoefficient    = 2;
	lifetimeMS         = 400;
	lifetimeVarianceMS = 200;
	sizes[0]           = 0.4;
	sizes[1]           = 0.6;
	sizes[2]           = 0.9;
	spinRandomMax      = 0;
	spinRandomMin      = 0;
	textureName        = "base/client/ui/brickIcons/1x1";
	times[1]           = 0.5;
	times[2]           = 1;
};

//Active emitter
datablock ParticleEmitterData(ND_DupliEmitterB)
{
	ejectionOffset   = -0.0;
	ejectionPeriodMS = 10;
	ejectionVelocity = 0;
	particles        = ND_DupliParticleB;
	periodVarianceMS = 2;
	thetaMin		 = 0.0;
	thetaMax         = 0.1;
	velocityVariance = 0;
	orientParticles  = true;
	phiVariance		 = 10;
};

//Duplicator image
datablock ShapeBaseImageData(ND_DupliImage : wandImage)
{
	projectile      = ND_DupliProjectile;
	showBricks      = true;
	colorShiftColor = "1 0.84 0 1";
	item            = ND_DupliItem;
	stateEmitter[1] = ND_DupliEmitterA;
	stateEmitter[3] = ND_DupliEmitterB;
	offset = "0 0 0";
};

//Resizable selection box
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

//Transparent cube to visualize bricks intersecting selection box
datablock StaticShapeData(ND_SelectionCubeOuterDts)
{
	shapeFile = $ND::ResourcePath @ "server/selectioncube_outer.dts";
};

//Inside cube (inverted normals) to visualize backfaces
datablock StaticShapeData(ND_SelectionCubeInnerDts)
{
	shapeFile = $ND::ResourcePath @ "server/selectioncube_inner.dts";
};

//Small cube to create solid edges
datablock StaticShapeData(ND_SelectionCubeBorderDts)
{
	shapeFile = $ND::ResourcePath @ "server/selectioncube_border.dts";
};

//Empty shape to hold shapename
datablock StaticShapeData(ND_ShapeNameDts)
{
	shapeFile = "base/data/shapes/empty.dts";
};
