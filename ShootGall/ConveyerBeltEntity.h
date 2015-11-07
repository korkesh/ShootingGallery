#ifndef CONVEYERBELt_H_
#define CONVEYERBELt_H_
#pragma once

#include "DynamicEntity.h"
class ConveyerBeltEntity :	public DynamicEntity
{
	// speed of rotation
	int speed;
	bool clockwise;

	
	int dispHeight;

	// properties of sprite
	int picWidth;
	int picHeight;

	// total distance in pixels
	int totalDistance;

	// time period based on the speed
	float timePeriod;

	// time we spend waiting in frame
	float timeInFrame;

	// time it takes to move atleast one frame
	float timePerPixel;

	// the main dimensions of conveyerbelt itself
	sf::IntRect dimensions;

public:

	static GameEntity *CreateConveyerBeltEntity(Editor::EntityType type, int ID,
		std::map<std::string, std::string> &props);

	void update(float dt) override;

	ConveyerBeltEntity();
	~ConveyerBeltEntity();

	//Draw method inherited from sf::Drawable.
	void draw(sf::RenderTarget &target, sf::RenderStates states) const override;

protected:
	ConveyerBeltEntity(int id);
};

#endif