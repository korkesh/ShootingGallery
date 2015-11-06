#pragma once
#ifndef DYNAMIC_LINE_SIN_H
#define DYNAMIC_LINE_SIN_H
#pragma once
#include "DynamicEntity.h"
//line sine is dynamic since it moves
class DynamicLineSine : public DynamicEntity
{

public:

	DynamicLineSine();
	~DynamicLineSine();

	static GameEntity *CreateLineSineEntity(Editor::EntityType type, int ID,
		std::map<std::string, std::string> &props);

	//update since things move
	void update(float dt) override;

	//how fast
	int speed;
	//amplitude
	int a;
	//period
	int b;
	//up or down from line
	int c;
	//initial y value
	int startingY;
	//are they just going up and down in their spot or more like movment on sin wave
	bool Static;
	//how long has it been
	float timeElpased;

protected:

	//Don't call me from anywhere but the factory methods!
	DynamicLineSine(int id);

private:
	//The entity that will be rendered to the screen. 
	std::vector<sf::Shape*> m_drawShapes;
	//keeps track of the dirction the current shape is moving
	std::vector<bool> direction;

	//Draw method inherited from sf::Drawable.
	virtual void draw(sf::RenderTarget &target, sf::RenderStates states) const;
};

#endif
