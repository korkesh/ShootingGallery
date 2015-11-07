#include "GameEntity.h"

#include <string>
#include <map>

#include <SFML/Graphics.hpp>

#include "tinyxml2.h"
#include "StringUtils.h"

#include "../Editor/EntityTypes.cs"

#include "StaticEntity.h"
#include "DynamicEntity.h"
#include "DynamicLineSine.h"
#include "ConveyerBeltEntity.h"
#define _USE_MATH_DEFINES 
#include <cmath>

//Factory method for creating a static rectangle. It gets drawn to a position and has a 
// size, an outline color, and a fill color. Static entities aren't expected to move.
//The string of properties must contain the necessary values - the required props can
// be found by checking EntityTypes.cs. If a property is missing or the object cannot be
// instantiated for any reason, the object is destroyed and NULL is returned.
//MIDTERM: You need to add a new property to the rectangles which sets the outline
// thickness. In the event an outline thickness is not found, you should output
// a warning message and set the thickness to a default value (1.0f). 
GameEntity *StaticEntity::CreateRectangularEntity(Editor::EntityType type, int ID,
	std::map<std::string, std::string> &props) 
{
	//Create the new entity and set up the shape.
	StaticEntity *ge = new StaticEntity(ID);
	ge->m_drawShape = new sf::RectangleShape();
	ge->m_drawShape->setOutlineThickness(1.0f);
	//is never curtain
	ge->isCurtain = false;
	//Check that all properties are present, convert them from strings to the required data type,
	// and set up the object. Note that ideally the order in which the properties are processed should
	// not matter.
	bool propMissing = false;
	for (std::size_t i = 0; i < Editor::EntityProperties[(int)type].size(); i++) {
		std::string propName = Editor::EntityProperties[(int)type][i];
		std::map<std::string, std::string>::iterator it = props.find(propName);
		//Currently we fail as soon as we miss a single value - if we have optional parameters we will
		// need to make this logic a bit more complex.
		if (it == props.end()) {
			propMissing = true;
			delete ge;
			return NULL;
		}
		else {
			//If the property is found, convert it to the appropriate type and initialise the object with
			// the value.
			if (propName == "Dimensions") {
				sf::IntRect dim = StringUtils::FromString<sf::IntRect>(it->second);
				ge->m_drawShape->setPosition(sf::Vector2f((float)dim.left, (float)dim.top));
				((sf::RectangleShape *)(ge->m_drawShape))->setSize(sf::Vector2f((float)dim.width, (float)dim.height));
			}
			else if (propName == "FillColor") {
				ge->m_drawShape->setFillColor(StringUtils::FromString<sf::Color>(it->second));
			}
			else if (propName == "OutlineColor") {
				ge->m_drawShape->setOutlineColor(StringUtils::FromString<sf::Color>(it->second));
			}
		}
	}
	//Return the initialised game entity.
	return ge;
}

//MIDTERM: To be completed.
GameEntity *StaticEntity::CreateCircularEntity(Editor::EntityType type, int ID,
	std::map<std::string, std::string> &props) {
	StaticEntity *ge = new StaticEntity(ID);
	ge->m_drawShape = new sf::CircleShape();

	//MIDTERM: You got this right?
	ge->m_drawShape->setOutlineThickness(1.0f);
	//is never curtain
	ge->isCurtain = false;
	//Check that all properties are present, convert them from strings to the required data type,
	// and set up the object. Note that ideally the order in which the properties are processed should
	// not matter.
	bool propMissing = false;
	for (std::size_t i = 0; i < Editor::EntityProperties[(int)type].size(); i++) {
		std::string propName = Editor::EntityProperties[(int)type][i];
		std::map<std::string, std::string>::iterator it = props.find(propName);
		//Currently we fail as soon as we miss a single value - if we have optional parameters we will
		// need to make this logic a bit more complex.
		if (it == props.end()) {
			propMissing = true;
			delete ge;
			return NULL;
		}
		else {
			//If the property is found, convert it to the appropriate type and initialise the object with
			// the value.
			if (propName == "Position") {
				sf::Vector2<float> point = StringUtils::FromString<sf::Vector2<float>>(it->second);
				ge->m_drawShape->setPosition(sf::Vector2f((float)point.x, (float)point.y));
			}
			else if (propName == "Radius") {
				float radius = StringUtils::FromString<float>(it->second);
				((sf::CircleShape *)(ge->m_drawShape))->setRadius(radius);
			}
			else if (propName == "FillColor") {
				ge->m_drawShape->setFillColor(StringUtils::FromString<sf::Color>(it->second));
			}
			else if (propName == "OutlineColor") {
				ge->m_drawShape->setOutlineColor(StringUtils::FromString<sf::Color>(it->second));
			}
		}
	}
	//Return the initialised game entity.
	return ge;
}

//creating Sprtie entity
GameEntity *StaticEntity::CreateSpriteEntity(Editor::EntityType type, int ID,
	std::map<std::string, std::string> &props)
{
	StaticEntity *ge = new StaticEntity(ID);
	ge->m_drawShape = new sf::RectangleShape();

	ge->type = 3;

	//has no outline
	ge->m_drawShape->setOutlineThickness(0.0f);
	//Check that all properties are present, convert them from strings to the required data type,
	// and set up the object. Note that ideally the order in which the properties are processed should
	// not matter.
	bool propMissing = false;

	for (std::size_t i = 0; i < Editor::EntityProperties[(int)type].size(); i++)
	{
		std::string propName = Editor::EntityProperties[(int)type][i];
		std::map<std::string, std::string>::iterator it = props.find(propName);
		//Currently we fail as soon as we miss a single value - if we have optional parameters we will
		// need to make this logic a bit more complex.
		if (it == props.end()) {
			propMissing = true;
			delete ge;
			return NULL;
		}
		else {

			//gets name of image
			if (propName == "SpriteName") {

				std::string name = (it->second);
				//if it is the curtain image set isCurtain to true else false
				if (name == "Curtain.fw.png")
				{
					ge->isCurtain = true;
				}
				else
				{
					ge->isCurtain = false;
				}
				//loads and sets proper image as texture
				name = "../Resources/Assets/" + name;
				sf::Texture *tex = new sf::Texture();
				tex->loadFromFile(name, sf::IntRect());
				ge->m_drawShape->setTexture(tex);

			}
			//gets dim of the rectangle for the image
			if (propName == "Dimensions") {
				sf::IntRect dim = StringUtils::FromString<sf::IntRect>(it->second);
				ge->m_drawShape->setPosition(sf::Vector2f((float)dim.left, (float)dim.top));
				((sf::RectangleShape *)(ge->m_drawShape))->setSize(sf::Vector2f((float)dim.width, (float)dim.height));
				ge->startingY = dim.top;

			}
			//checks if the image should be reversed(mirrored)
			if (propName == "Flipped") {
				std::string flip = (it->second);
				if (flip == "True")
				{
					//flips image
					ge->m_drawShape->setOrigin(ge->m_drawShape->getLocalBounds().width, 0);
					ge->m_drawShape->setScale(-1, 1);
				}
			}
			else if (propName == "OutlineColor") {
				ge->m_drawShape->setOutlineColor(StringUtils::FromString<sf::Color>(it->second));
			}
		}
	}

	//Return the initialised game entity.
	return ge;
}

//creating linetrack
GameEntity *DynamicEntity::CreateLineTrackEntity(Editor::EntityType type, int ID,
	std::map<std::string, std::string> &props)
{
	DynamicEntity *ge = new DynamicEntity(ID);

	ge->type = 4;
	//is never curtain
	ge->isCurtain = false;
	sf::IntRect dim;
	int picWidth;
	int picHeight;

	std::string name;
	//direction the images move
	bool clockwise;

	bool propMissing = false;

	for (std::size_t i = 0; i < Editor::EntityProperties[(int)type].size(); i++)
	{
		std::string propName = Editor::EntityProperties[(int)type][i];
		std::map<std::string, std::string>::iterator it = props.find(propName);
		//Currently we fail as soon as we miss a single value - if we have optional parameters we will
		// need to make this logic a bit more complex.
		if (it == props.end()) {
			propMissing = true;
			delete ge;
			return NULL;
		}
		else {

			//If the property is found, convert it to the appropriate type and initialise the object with
			// the value.
			if (propName == "SpriteName")
			{
				name = (it->second);
				name = "../Resources/Assets/" + name;
		

			}
			//dim of total rec
			else if (propName == "Dimensions")
			{
				dim = StringUtils::FromString<sf::IntRect>(it->second);
			}
			//how high will they go
			else if (propName == "DispHeight")
			{
				ge->dispHeight = StringUtils::FromString<sf::Int32>(it->second);
			}
			//picture height
			else if (propName == "PicHeight")
			{
				picHeight = StringUtils::FromString<sf::Int32>(it->second);
				//ge->m_drawShape->setOutlineColor(StringUtils::FromString<sf::Color>(it->second));
			}
			//pic width
			else if (propName == "PicWidth")
			{
				picWidth = StringUtils::FromString<sf::Int32>(it->second);
			}
			//how fast they move
			else if (propName == "Speed")
			{
				ge->speed = StringUtils::FromString<sf::Int32>(it->second);
			}
			//direction
			else if (propName == "Clockwise")
			{
				clockwise = true;
			}
		}
	}


	sf::IntRect rec = sf::IntRect(dim.left, dim.top, picWidth, picHeight);
	ge->startingY = dim.top;

	sf::Texture *tex = new sf::Texture();
	tex->loadFromFile(name, sf::IntRect());

	//creates new ractshape for every image needed
	for (size_t i = 0; i < dim.width / picWidth; i++)
	{
		sf::RectangleShape *m_drawShape = new sf::RectangleShape();
		m_drawShape->setTexture(tex);

		m_drawShape->setPosition(sf::Vector2f(rec.left, rec.top));

		((sf::RectangleShape *)(m_drawShape))->setSize(sf::Vector2f((float)rec.width, (float)rec.height));

		ge->m_drawShapes.push_back(m_drawShape);

		rec.left += (picWidth);
	}

	return ge;
}

GameEntity *ConveyerBeltEntity::CreateConveyerBeltEntity(Editor::EntityType type, int ID,
	std::map<std::string, std::string> &props)
{
	ConveyerBeltEntity *ge = new ConveyerBeltEntity(ID);

	ge->type = 5;
	ge->isCurtain = false;
	std::string name;
	bool clockwise;

	//MIDTERM: You got this right?
	//ge->m_drawShape->setOutlineThickness(0.0f);
	//Check that all properties are present, convert them from strings to the required data type,
	// and set up the object. Note that ideally the order in which the properties are processed should
	// not matter.
	bool propMissing = false;

	for (std::size_t i = 0; i < Editor::EntityProperties[(int)type].size(); i++)
	{
		std::string propName = Editor::EntityProperties[(int)type][i];
		std::map<std::string, std::string>::iterator it = props.find(propName);
		//Currently we fail as soon as we miss a single value - if we have optional parameters we will
		// need to make this logic a bit more complex.
		if (it == props.end()) {
			propMissing = true;
			delete ge;
			return NULL;
		}
		else {

			//If the property is found, convert it to the appropriate type and initialise the object with
			// the value.
			if (propName == "SpriteName")
			{
				name = (it->second);
				name = "../Resources/Assets/" + name;

			}
			else if (propName == "Dimensions")
			{
				ge->dimensions = StringUtils::FromString<sf::IntRect>(it->second);
			}
			else if (propName == "DispHeight")
			{
				ge->dispHeight = StringUtils::FromString<sf::Int32>(it->second);
			}
			else if (propName == "PicHeight")
			{
				ge->picHeight = StringUtils::FromString<sf::Int32>(it->second);
			}
			else if (propName == "PicWidth")
			{
				ge->picWidth = StringUtils::FromString<sf::Int32>(it->second);
			}
			else if (propName == "Speed")
			{
				ge->speed = StringUtils::FromString<sf::Int32>(it->second);
			}
			else if (propName == "Clockwise")
			{
				std::string cw = (it->second);
				if (cw == "True")
				{
					ge->clockwise = true;
				}
				else
				{
					ge->clockwise = false;
				}
			}
		}
	}

	// remove any extra pixels in width 
	int changedWidth = ((int)ge->dimensions.width / (int)ge->picWidth) * ge->picWidth;

	// remove any extra pixels in height
	int changedHeight = ((int)ge->dimensions.height / (int)ge->picHeight) * ge->picHeight;

	if (changedHeight == 0)
	{
		changedHeight = 1;
	}

	//change the dimension such that there is no gap
	ge->dimensions = sf::IntRect(ge->dimensions.left, ge->dimensions.top, changedWidth, changedHeight);


	sf::IntRect rec = sf::IntRect(ge->dimensions.left, ge->dimensions.top, ge->picWidth, ge->picHeight);
	ge->startingY = ge->dimensions.top;

	// calculte total number of pixels that needs to be travelled to cover the whole path
	ge->totalDistance = (2 * ge->dimensions.width) + (2 * ge->dimensions.height);

	// calculte the time period depending upon the speed
	ge->timePeriod = 50000.0f / (float)ge->speed;

	// from totalNumberOfPixels and time period. Calculte how much time will it take to travel one pixel
	ge->timePerPixel = (float)ge->timePeriod / (float)ge->totalDistance;




	// define a reference Texture from spriteName
	// that can be assigned to each sprite in this conveyerBelt
	sf::Texture *tex = new sf::Texture();
	tex->loadFromFile(name, sf::IntRect());

	// create sprite for top layer
	for (size_t i = 0; i < ge->dimensions.width / ge->picWidth; i++)
	{
		sf::RectangleShape *m_drawShape = new sf::RectangleShape();
		m_drawShape->setTexture(tex);

		m_drawShape->setPosition(sf::Vector2f(rec.left, rec.top));

		((sf::RectangleShape *)(m_drawShape))->setSize(sf::Vector2f((float)rec.width, (float)rec.height));

		ge->m_drawShapes.push_back(m_drawShape);

		rec.left += (ge->picWidth);
	}

	rec.left = ge->dimensions.left + ge->dimensions.width;

	// create sprites for right layer
	for (size_t i = 0; i < ge->dimensions.height / ge->picHeight; i++)
	{
		sf::RectangleShape *m_drawShape = new sf::RectangleShape();
		m_drawShape->setTexture(tex);

		m_drawShape->setPosition(sf::Vector2f(rec.left, rec.top));

		((sf::RectangleShape *)(m_drawShape))->setSize(sf::Vector2f((float)rec.width, (float)rec.height));

		ge->m_drawShapes.push_back(m_drawShape);

		rec.top += (ge->picHeight);

	}

	rec.top = ge->dimensions.top + ge->dimensions.height;
	// create sprite for bottom layer
	for (size_t i = 0; i < ge->dimensions.width / ge->picWidth; i++)
	{
		sf::RectangleShape *m_drawShape = new sf::RectangleShape();
		m_drawShape->setTexture(tex);

		m_drawShape->setPosition(sf::Vector2f(rec.left, rec.top));

		((sf::RectangleShape *)(m_drawShape))->setSize(sf::Vector2f((float)rec.width, (float)rec.height));

		ge->m_drawShapes.push_back(m_drawShape);

		rec.left -= (ge->picWidth);
	}

	rec.left = ge->dimensions.left;

	// create sprites for left layer
	for (size_t i = 0; i < ge->dimensions.height / ge->picHeight; i++)
	{
		sf::RectangleShape *m_drawShape = new sf::RectangleShape();
		m_drawShape->setTexture(tex);

		m_drawShape->setPosition(sf::Vector2f(rec.left, rec.top));

		((sf::RectangleShape *)(m_drawShape))->setSize(sf::Vector2f((float)rec.width, (float)rec.height));

		ge->m_drawShapes.push_back(m_drawShape);

		rec.top -= (ge->picHeight);

	}

	return ge;
}

//creates line sine
GameEntity *DynamicLineSine::CreateLineSineEntity(Editor::EntityType type, int ID,
	std::map<std::string, std::string> &props)
{
	DynamicLineSine *ge = new DynamicLineSine(ID);

	ge->type = 6;
	ge->isCurtain = false;
	sf::IntRect dim;
	int picWidth;
	int picHeight;

	std::string name;

	bool propMissing = false;

	for (std::size_t i = 0; i < Editor::EntityProperties[(int)type].size(); i++)
	{
		std::string propName = Editor::EntityProperties[(int)type][i];
		std::map<std::string, std::string>::iterator it = props.find(propName);
		//Currently we fail as soon as we miss a single value - if we have optional parameters we will
		// need to make this logic a bit more complex.
		if (it == props.end()) {
			propMissing = true;
			delete ge;
			return NULL;
		}
		else {

			//If the property is found, convert it to the appropriate type and initialise the object with
			// the value.
			//gets name for image
			if (propName == "SpriteName")
			{
				name = (it->second);
				name = "../Resources/Assets/" + name;
		

			}
			//dim of total rect
			else if (propName == "Dimensions")
			{
				dim = StringUtils::FromString<sf::IntRect>(it->second);
			}
			//amplitude
			else if (propName == "a")
			{
				ge->a = StringUtils::FromString<sf::Int32>(it->second);
			}
			//pic height
			else if (propName == "PicHeight")
			{
				picHeight = StringUtils::FromString<sf::Int32>(it->second);
				
			}
			//picWidth
			else if (propName == "PicWidth")
			{
				picWidth = StringUtils::FromString<sf::Int32>(it->second);
			}
			//how fast it moves
			else if (propName == "Speed")
			{
				ge->speed = StringUtils::FromString<sf::Int32>(it->second);
			}
			//period
			else if (propName == "b")
			{
				ge->b = StringUtils::FromString<sf::Int32>(it->second);
			}
			//distance down
			else if (propName == "c")
			{
				ge->c = StringUtils::FromString<sf::Int32>(it->second);
			}
			//move in sin wave or just up and down in pos
			else if (propName == "Static")
			{
				std::string s = (it->second);
				if (s == "True")
				{
					ge->Static = true;
				}
				else
				{
					ge->Static = false;
				}
			}
		}
	}


	sf::IntRect rec = sf::IntRect(dim.left, dim.top, picWidth, picHeight);
	ge->startingY = dim.top;

	sf::Texture *tex = new sf::Texture();
	tex->loadFromFile(name, sf::IntRect());
	for (size_t i = 0; i < dim.width / picWidth; i++)
	{
		sf::RectangleShape *m_drawShape = new sf::RectangleShape();
		m_drawShape->setTexture(tex);
		//calculates postion based on sin wave
		double mid = (2 * 3.14159265358979323846) / (dim.width * 1 / ge->b);
		int d = (int)(2 * ge->a*_CMATH_::sin(mid*rec.left) + ge->c + rec.top);
		//use the derivative to figure out if it should be going up or down
		int z = (int)(2 * mid* ge->a*_CMATH_::cos(mid*rec.left));
		bool dir;
		// dir false means down , dir true means up
		if (z < 0)
		{
			dir = false;
		}
		else if (z > 0)
		{
			dir = true;
		}
		//if the dirivative is 0 meaning its at the top or bottom move over 1 to figure out dir
		else if (z == 0)
		{
			z = 2 * mid* ge->a*_CMATH_::cos(mid*rec.left + 1);
			if (z < 0)
			{
				dir = false;
			}
			else if (z > 0)
			{
				dir = true;
			}
		}

		//set the position
		m_drawShape->setPosition(sf::Vector2f(rec.left, d));

		((sf::RectangleShape *)(m_drawShape))->setSize(sf::Vector2f((float)rec.width, (float)rec.height));
		//add the entities and direction to the respective vectors
		ge->m_drawShapes.push_back(m_drawShape);
		ge->direction.push_back(dir);
		//increase in the x axis
		rec.left += (picWidth);
	}

	return ge;
}


//MIDTERM: You will need additional factory functions with the correct signature to handle 
// any new gameentity types you create.

//Collection of factory methods mapped to entity types in EntityTypes.cs. Put the address of the methods
// you want at the appropriate spot in the array. If you want to rewrite this to use the C++11 standard
// feel free. Please bring any troubles with debugging to Will.
std::vector<GameEntity *(*)(Editor::EntityType type, int ID, std::map<std::string, std::string> &props)> 
factories = {
	NULL,
	&StaticEntity::CreateRectangularEntity,
	&StaticEntity::CreateCircularEntity,
	&StaticEntity::CreateSpriteEntity,
	&DynamicEntity::CreateLineTrackEntity,
	&ConveyerBeltEntity::CreateConveyerBeltEntity,
	&DynamicLineSine::CreateLineSineEntity

};


//Don't call this unless you have a very good reason, it's declared protected for a reason.
GameEntity::GameEntity(int id) : m_id(id)
{
}


GameEntity::~GameEntity()
{
	
}

//Loads the object from an xml element. Note that this method does not care what type of
// game entity it is loading - it uses the factories collection to ensure the correct
// factory method is called.
GameEntity *GameEntity::loadFromXml(tinyxml2::XMLElement *element) {

	std::string att = element->Attribute("Type");
	Editor::EntityType t = (Editor::EntityType)StringUtils::FromString<int>(att);
	att = element->Attribute("ID");
	int id = StringUtils::FromString<int>(att);
	std::map<std::string, std::string> properties;
	tinyxml2::XMLElement *propEl = element->FirstChildElement();
	while (propEl != NULL) {
		properties[propEl->Name()] = propEl->GetText();
		propEl = propEl->NextSiblingElement();
	}
	return factories[(int)t](t, id, properties);
}

