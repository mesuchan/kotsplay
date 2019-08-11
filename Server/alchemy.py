from lxml import etree

class Ingridient:
  def __init__(self, id, name):
    self.id = id
    self.name = name

class Recipe:
  def __init__(self, id, name, ingredients):
    self.id = id
    self.name = name
    self.ingredients = ingredients


class Book:
  def __init__(self, name, ingredients, recipes):
    self.name = name
    self.ingredients = ingredients
    self.recipes = recipes


def load_book(path):
  with open(path) as f:
    contents = "".join(f.readlines())
    root = etree.XML(contents)
    recipes = []
    ingredients = []
    for element in root.iter():
      if element.tag == "Ingredient":
        id = int(element.attrib["id"])
        name = element.attrib["name"]
        ingredients.append(Ingridient(id, name))
      elif element.tag == "Recipe":
        id = int(element.attrib["id"])
        name = element.attrib["name"]
        recipeIngredients = set()
        for subel in element.iter():
          if subel.tag == "IngredientId":
            recipeIngredients.add(int(subel.text))
        recipes.append(Recipe(id, name, recipeIngredients))
        ingredients.append(Recipe(id, name, recipeIngredients))
    return Book(root.attrib["name"], ingredients, recipes)


def find_recipe(book, ingredients):
  for recipe in book.recipes:
    if recipe.ingredients == ingredients:
      return recipe
    
