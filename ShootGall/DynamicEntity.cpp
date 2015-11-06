#include "DynamicEntity.h"
#include <iostream>
#include <SFML/Graphics/RenderTarget.hpp>

//Constructor that initialises things to null. You should only call this in 
// a factory method. Note that draw() makes it very dangerous to use an 
// an improperly loaded entity of this type since the draw shape is NULL.
DynamicEntity::DynamicEntity(int id) : GameEntity(id), m_drawShapes(NULL)
{
	up = true;
	timeElpased = 0;
}


DynamicEntity::~DynamicEntity()
{
	//Clean up.
	if (m_drawShapes.size() > 0)
	{
		for (size_t i = 0; i < m_drawShapes.size(); i++)
		{
			sf::Shape *m_drawShape = m_drawShapes[i];
			if (m_drawShape != NULL)
			{
				delete m_drawShape;
				m_drawShape = NULL;
			}
		}
	}
	
}

//Render the draw shape to screen.
void DynamicEntity::draw(sf::RenderTarget &target, sf::RenderStates states) const
{

	if (m_drawShapes.size() > 0)
	{
		for (size_t i = 0; i < m_drawShapes.size(); i++)
		{
			sf::Shape *m_drawShape = m_drawShapes[i];
			target.draw(*m_drawShape, states);
		}
	}
}

//for linetrack
void DynamicEntity::update(float dt)
{
	//check if enough time has passed
	if (timeElpased < 100/speed)
	{
		timeElpased += dt;
	}
	else
	{
		//reset timer
		timeElpased = 0;
	

		if (m_drawShapes.size() > 0)
		{

			sf::Shape *first = m_drawShapes[0];
			int stY = first->getPosition().y;
			//check if the first image has gone too high or to low .. if so swap the direction of all
			if ((stY - (dt * speed) < startingY - dispHeight) && up)
			{
				up = false;
			}
			if (stY + (dt * speed) > (startingY + dispHeight) && !up)
			{
				up = true;
			}


			//moves each image in correct direction by 1
			for (size_t i = 0; i < m_drawShapes.size(); i++)
			{
				sf::Shape *m_drawShape = m_drawShapes[i];
				int pos = m_drawShape->getPosition().y;
				switch (i % 2)
				{
					// its evens
				case 0:
					//first image always starts up
					if (up)
					{
						pos -= 1;
					}
					else
					{
						pos += 1;
					}

					break;

				case 1:
					if (up)
					{
						pos += 1;
					}
					else
					{
						pos -= 1;
					}
					break;
				default:
					break;
				}
				//set the new position
				m_drawShape->setPosition(m_drawShape->getPosition().x, pos);

			}
		}
	}
}
