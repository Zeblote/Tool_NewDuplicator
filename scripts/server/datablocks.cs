// * ######################################################################
// *
// *    New Duplicator - Scripts - Server
// *    Datablocks
// *
// *    -------------------------------------------------------------------
// *    Creates datablocks required for the new duplicator
// *
// * ######################################################################

//Duplicator to hold in your hand
///////////////////////////////////////////////////////////////////////////

//Duplicator Item
datablock ItemData(NewDuplicatorItem)
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
	image           = "NewDuplicatorImage";
	shapeFile       = "base/data/shapes/wand.dts";
	uiName          = "Duplicator";
};

//Particles for explosion
datablock ParticleData(NewDuplicatorExplosionParticle)
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
datablock ParticleEmitterData(NewDuplicatorExplosionEmitter)
{
	ejectionOffset   = 0.5;
	ejectionPeriodMS = 4;
	ejectionVelocity = 3;
	particles        = NewDuplicatorExplosionParticle;
	periodVarianceMS = 2;
	thetaMax         = 180;
	velocityVariance = 0;
};

//Explosion 
datablock ExplosionData(NewDuplicatorExplosion)
{
	camShakeDuration = 0.5;
	camShakeFreq     = "1 1 1";
	emitter[0]       = NewDuplicatorExplosionEmitter;
	faceViewer       = 1;
	lifetimeMS       = 180;
	lightEndRadius   = 0;
	lightStartColor  = "0 0 0 0";
	lightStartRadius = 0;
	shakeCamera      = 1;
	soundProfile     = "wandHitSound";
};

//Projectile to make explosion
datablock ProjectileData(NewDuplicatorProjectile)
{
	bounceElasticity = 0;
	bounceFriction   = 0;
	explodeOnDeath   = 1;
	explosion        = NewDuplicatorExplosion;
	fadeDelay        = 2;
	gravityMod       = 0;
	lifetime         = 0;
	range            = 10;
};

//Idle particles
datablock ParticleData(NewDuplicatorParticleA)
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
datablock ParticleEmitterData(NewDuplicatorEmitterA)
{
	ejectionOffset   = 0.09;
	ejectionPeriodMS = 50;
	ejectionVelocity = 0.2;
	particles        = NewDuplicatorParticleA;
	periodVarianceMS = 2;
	thetaMax         = 180;
	velocityVariance = 0;
};

//Active particles
datablock ParticleData(NewDuplicatorParticleB)
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
datablock ParticleEmitterData(NewDuplicatorEmitterB)
{
	ejectionOffset   = -0.0;
	ejectionPeriodMS = 10;
	ejectionVelocity = 0;
	particles        = NewDuplicatorParticleB;
	periodVarianceMS = 2;
	thetaMin		 = 0.0;
	thetaMax         = 0.1;
	velocityVariance = 0;
	orientParticles  = true;
	phiVariance		 = 10;
};

//Duplicator image
datablock ShapeBaseImageData(NewDuplicatorImage : wandImage)
{
	showBricks      = true;
	offset          = "0 0 0";
	colorShiftColor = "1 0.84 0 1";
	item            = NewDuplicatorItem;
	stateEmitter[1] = NewDuplicatorEmitterA;
	stateEmitter[3] = NewDuplicatorEmitterB;
	projectile      = NewDuplicatorProjectile;
};

//Special blue duplicator for plant mode
///////////////////////////////////////////////////////////////////////////

//Particles for explosion
datablock ParticleData(NewDuplicatorBlueExplosionParticle : NewDuplicatorExplosionParticle)
{
	colors[0] = "0 0.25 1 0.9";
	colors[1] = "0 0.25 1 0.7";
	colors[2] = "0 0.25 1 0.5";
};

//Emitter for explosion
datablock ParticleEmitterData(NewDuplicatorBlueExplosionEmitter : NewDuplicatorExplosionEmitter)
{
	particles = NewDuplicatorBlueExplosionParticle;
};

//Explosion 
datablock ExplosionData(NewDuplicatorBlueExplosion : NewDuplicatorExplosion)
{
	emitter[0] = NewDuplicatorBlueExplosionEmitter;
};

//Projectile to make explosion
datablock ProjectileData(NewDuplicatorBlueProjectile : NewDuplicatorProjectile)
{
	explosion = NewDuplicatorBlueExplosion;
};

//Idle particles
datablock ParticleData(NewDuplicatorBlueParticleA : NewDuplicatorParticleA)
{
	colors[0] = "0 0.25 1 0.9";
	colors[1] = "0 0.25 1 0.7";
	colors[2] = "0 0.25 1 0.5";
};

//Idle emitter
datablock ParticleEmitterData(NewDuplicatorBlueEmitterA : NewDuplicatorEmitterA)
{
	particles = NewDuplicatorBlueParticleA;
};

//Active particles
datablock ParticleData(NewDuplicatorBlueParticleB : NewDuplicatorParticleB)
{
	colors[0] = "0 0.25 1 0.9";
	colors[1] = "0 0.25 1 0.7";
	colors[2] = "0 0.25 1 0.5";
};

//Active emitter
datablock ParticleEmitterData(NewDuplicatorBlueEmitterB : NewDuplicatorEmitterB)
{
	particles = NewDuplicatorBlueParticleB;
};

//Duplicator image
datablock ShapeBaseImageData(NewDuplicatorBlueImage : NewDuplicatorImage)
{
	colorShiftColor = "0 0.25 1 1";
	stateEmitter[1] = NewDuplicatorBlueEmitterA;
	stateEmitter[3] = NewDuplicatorBlueEmitterB;
	projectile      = NewDuplicatorBlueProjectile;
};

//Resizable selection and highlight box
///////////////////////////////////////////////////////////////////////////

//Transparent cube to visualize bricks intersecting selection box
datablock StaticShapeData(ND_SelectionCubeOuter)
{
	shapeFile = $ND::ResourcePath @ "server/selectioncube_outer.dts";
};

//Inside cube (inverted normals) to visualize backfaces
datablock StaticShapeData(ND_SelectionCubeInner)
{
	shapeFile = $ND::ResourcePath @ "server/selectioncube_inner.dts";
};

//Small cube to create solid edges
datablock StaticShapeData(ND_SelectionCubeBorder)
{
	shapeFile = $ND::ResourcePath @ "server/selectioncube_border.dts";
};

//Empty shape to hold shapename
datablock StaticShapeData(ND_SelectionCubeShapeName)
{
	shapeFile = "base/data/shapes/empty.dts";
};
