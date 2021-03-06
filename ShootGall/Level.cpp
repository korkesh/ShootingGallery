#include "Level.h"

#include <SFML/Graphics.hpp>
#include "tinyxml2.h"

Level::Level()
{
	
}

//Clean up!
Level::~Level()
{
	for (std::size_t i = 0; i < m_entities.size(); i++) {
		delete m_entities[i];
		m_entities[i] = NULL;
	}
}

//Attempts to load the level. Could use some more error checking code.
bool Level::loadFromFile(std::string filename) {
	tinyxml2::XMLDocument doc;
	//Maybe we should confirm the file loaded before we proceed?
	doc.LoadFile(filename.c_str());
	//Null pointer exceptions? Those never happen!
	// Also maybe you might want to specify the name of the elements.
	tinyxml2::XMLElement *rootEl = doc.FirstChildElement();
	//Iterate through each child element (which is hopefully a GameEntity
	// element) and attempt to create a game entity. If the entity is 
	// created add it to the collection of entities.
	tinyxml2::XMLElement *entityEl = rootEl->FirstChildElement();
	while (entityEl != NULL) {
		GameEntity *ge = GameEntity::loadFromXml(entityEl);
		if (ge != NULL) {
			m_entities.push_back(ge);
		}
		else {
			//Output error message.

		}
		//Get next element
		entityEl = entityEl->NextSiblingElement();
	} 
	return true;
}

void Level::draw(sf::RenderTarget &target, sf::RenderStates states) const {
	//Render each entity in order.
	for (std::size_t i = m_entities.size(); i > 0; --i) {
		target.draw(*m_entities[i - 1]);
	}
}

//update the level
void Level::update(float dt)
{
	//if their are enitites in level
	if (m_entities.size() > 0)
	{
		for (size_t i = 0; i < m_entities.size(); i++)
		{
			GameEntity *entity = m_entities[i];

			//movable entities all have type greater than 3
			if (entity->type > 3)
			{
				//call that entities update
				entity->update(dt);
			}

		}
	}
}

//code to move the curtain
void Level::moveCurtain(float a)
{
	GameEntity *entity = NULL;
	bool found = false;

	if (m_entities.size() > 0)
	{
		for (size_t i = 0; i < m_entities.size(); i++)
		{
			//checks if it has fount the entity that has iscurtain as true
			if (m_entities[i]->isCurtain)
			{
				entity = m_entities[i];
				found = true;
				break;
			}
		}
	}

	if (entity != NULL)
	{
		//updates if found , since no other static entity needs an update I made the static update specific for the curtain
		entity->update(a);
	}
}
