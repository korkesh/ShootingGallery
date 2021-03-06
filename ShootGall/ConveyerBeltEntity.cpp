#include "ConveyerBeltEntity.h"
#include <iostream>
#include <cmath>


ConveyerBeltEntity::ConveyerBeltEntity(int id) : DynamicEntity(id)
{
	timeElpased = 0;
	timeInFrame = 0;
}
void ConveyerBeltEntity::draw(sf::RenderTarget &target, sf::RenderStates states) const
{
	DynamicEntity::draw(target, states);
}


ConveyerBeltEntity::~ConveyerBeltEntity()
{
}

void ConveyerBeltEntity::update(float dt)
{
	// wait until the enough time has passed that we can add alteast one pixel to position	
	if (timeInFrame < timePerPixel)
	{		
		timeInFrame += dt;
		timeElpased += dt;
	}
	else
	{
		// reset the wait counter to zero
		timeInFrame = 0;

		// check if there is any entity
		if (m_drawShapes.size() > 0)
		{
			// iterate over all the entities to update them
			for (size_t i = 0; i < m_drawShapes.size(); i++)
			{
				// get entity and get its current X and Y position
				sf::Shape *m_drawShape = m_drawShapes[i];
				int posY = m_drawShape->getPosition().y;
				int posX = m_drawShape->getPosition().x;


				// there are 4 areas of movement for sqaure
				// first one moves the enitity right
				// second one moves the entity down
				// in third one, entity moves left
				// and lastly , entity moves back UP. It complestes one rotation and the cycle starts again

				// check if the entity is in first time region
				
 				if (posX >= dimensions.left && posX < (dimensions.left + dimensions.width)  && posY == dimensions.top)
				{
					// move left
					posX += 1;
				}
				else if (posX == (dimensions.left + dimensions.width) && posY >= dimensions.top && posY < (dimensions.top + dimensions.height))
				{
					// move entity down
					posY += 1;
				}
				else if (posX > dimensions.left && posY == (dimensions.top + dimensions.height))
				{
					// move entity left
					posX -= 1;
				}
				else if (posX == dimensions.left && posY >= dimensions.top)
				{
					// move entity back up
					posY -= 1;
				}
				
				if (timeElpased >= timePeriod)
				{
					timeElpased = 0;
				}
				
				// set the new position
				m_drawShape->setPosition(posX, posY);
				
			}
		}
	}
}
