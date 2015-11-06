#ifndef CONVEYERBELt_H_
#define CONVEYERBELt_H_
#pragma once

#include "DynamicEntity.h"
class ConveyerBeltEntity :	public DynamicEntity
{

	int speed;
	bool clockwise;

	int dispHeight;
	int picWidth;
	int picHeight;

	int totalDistance;

	float timePeriod;
	float t1;
	float t2;
	float t3;

	float timeForOneSprite;
	float timeInFrame;
	float timePerPixel;

	float dispTime;

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