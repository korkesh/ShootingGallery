#pragma once
#include "GameEntity.h"
/// <summary>
/// StaticEntities are placed in the world and never move. They can be fixed shapes, sprites,
/// or whatever else you can render using an sf::Shape object. You don't construct these directly
/// from a constructor, but should instead use a factory method, which are implemented over in GameEntity.cpp
/// instead of StaticEntity.cpp.
/// </summary>
/// <author>
/// original: Kevin Forest
/// </author>
class StaticEntity : public GameEntity
{
public:
	//Don't forget to clean up!
	virtual ~StaticEntity();
	//Create a Rectangle. Body is in GameEntity.h. If props does not contain the right data,
	// this will return NULL.
	static GameEntity *CreateRectangularEntity(Editor::EntityType type, int ID,
		std::map<std::string, std::string> &props);
	//Create a Circle. Body is in GameEntity.h. If props does not contain the right data,
	// this will return NULL. Needs to be fully implemented.
	static GameEntity *CreateCircularEntity(Editor::EntityType type, int ID,
		std::map<std::string, std::string> &props);

	//Create a Sprite. Body is in GameEntity.h. If props does not contain the right data,
	// this will return NULL. Needs to be fully implemented.
	static GameEntity *CreateSpriteEntity(Editor::EntityType type, int ID,
		std::map<std::string, std::string> &props);

	//initial y pos of image(normally the first)
	int startingY;

	//update only used for curtain
	void update(float dt) override
	{
		//checks time so that it doesnt move too fast
		if (deltatime > 100)
		{
			if (dt <= 0)
			{
				float cur = m_drawShape->getPosition().y;
				float ncur = cur - 1;
				//checks if it will go to high if so it doesnt change the pos
				if (ncur > startingY - 480)
				{
					m_drawShape->setPosition(m_drawShape->getPosition().x, ncur);
				}

			}
			else if (dt > 0)
			{
				float cur = m_drawShape->getPosition().y;
				float ncur = cur + 1;
				//checks if it will go to low(restriction is startingY)
				if (ncur < startingY)
				{
					m_drawShape->setPosition(m_drawShape->getPosition().x, ncur);
				}
			}
			deltatime = 0;
		}
		else
		{
			deltatime++;
		}

	}

protected:
	//Don't call me from anywhere but the factory methods!
	StaticEntity(int id);

	int deltatime = 0;
private:
	//The entity that will be rendered to the screen. 
	sf::Shape *m_drawShape;
	//Draw method inherited from sf::Drawable.
	virtual void draw(sf::RenderTarget &target, sf::RenderStates states) const;
};
