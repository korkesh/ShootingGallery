#ifndef DYNAMIC_ENTITY_H_
#define DYNAMIC_ENTITY_H_
#pragma once

#include "GameEntity.h"
//class for movable objects
class DynamicEntity :	public GameEntity
{

public:

	DynamicEntity();
	~DynamicEntity();

	static GameEntity *CreateLineTrackEntity(Editor::EntityType type, int ID,
		std::map<std::string, std::string> &props);

	//has update since it moves
	void update(float dt) override;

	//how fast it moves
	int speed;
	//how high it moves
	int dispHeight;
	//is the first picture going up
	bool up;
	//initial starting spot for y value
	int startingY;
	
	float timeElpased;

protected:

	//Don't call me from anywhere but the factory methods!
	DynamicEntity(int id);

private:
	//The entity that will be rendered to the screen. 
	std::vector<sf::Shape*> m_drawShapes;
	


	//Draw method inherited from sf::Drawable.
	virtual void draw(sf::RenderTarget &target, sf::RenderStates states) const;
};

#endif
