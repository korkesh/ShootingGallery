#include "DynamicLineSine.h"
#include <iostream>
#include <SFML/Graphics/RenderTarget.hpp>

//Constructor that initialises things to null. You should only call this in 
// a factory method. Note that draw() makes it very dangerous to use an 
// an improperly loaded entity of this type since the draw shape is NULL.
DynamicLineSine::DynamicLineSine(int id) : DynamicEntity(id), m_drawShapes(NULL)
{
	//up always starts as true
	up = true;
	//time always starts at 0
	timeElpased = 0;
}


DynamicLineSine::~DynamicLineSine()
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
void DynamicLineSine::draw(sf::RenderTarget &target, sf::RenderStates states) const
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

//updates for lineSin
void DynamicLineSine::update(float dt)
{
	//check if enough time has passed
	if (timeElpased < 100 / speed)
	{
		timeElpased += dt;
	}
	else
	{
		//reset timer
		timeElpased = 0;


		if (m_drawShapes.size() > 0)
		{
			//if images just moving in their spot (Basically same code from linetrack/dynamiceEntity update)
			if (Static)
			{
				sf::Shape *first = m_drawShapes[0];
				int stY = first->getPosition().y;

				if ((stY - 1 < startingY - 5) && up)
				{
				up = false;
				}
				if (stY +1 > (startingY + 20) && !up)
				{
				up = true;
				}

				for (size_t i = 0; i < m_drawShapes.size(); i++)
				{
					sf::Shape *m_drawShape = m_drawShapes[i];
					int pos = m_drawShape->getPosition().y;
					switch (i % 2)
					{
						// its evens
					case 0:

						if (up)
						{
							
							pos -= 1;
						}
						else
						{
					
							pos += 1;
						}

						break;

						// its odd
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

					m_drawShape->setPosition(m_drawShape->getPosition().x, pos);

				}
			}
			else
			{
				//movement more like sine wave
				for (size_t i = 0; i < m_drawShapes.size(); i++)
				{
					sf::Shape *m_drawShape = m_drawShapes[i];
					int pos = m_drawShape->getPosition().y;
					bool dir = direction[i];
					//checks if going up or down and then checks if it will hit the boundary
					if (dir == true)
					{
						if ((pos - 1) < startingY - (a * 2))
						{
							direction[i] = false;
						}

					}
					else
					{
						if (pos + 1 > startingY + (a * 2))
						{
							direction[i] = true;
						}
					}

					//moves it in correct direction by 1
					if (direction[i] == true)
					{
						pos -= 1;
					}
					else
					{
						pos += 1;
					}
					m_drawShape->setPosition(m_drawShape->getPosition().x, pos);

				}
			}
		}
	}
}
