-- Table: public."CookingAssistant.Ingredients"

-- DROP TABLE public."CookingAssistant.Ingredients";

CREATE TABLE public."CookingAssistant.Ingredients"
(
    "Id" serial NOT NULL,
	"ParentId" integer,
    "CategoryId" integer,
    "UserId" integer NOT NULL,
    "TaskId" integer,
    "Name" character varying(50) COLLATE pg_catalog."default",
    "MeasurementType" smallint,
    "ServingSize" smallint NOT NULL DEFAULT 100,
    "ServingSizeIsOneUnit" boolean NOT NULL DEFAULT FALSE,
    "Calories" numeric(4, 1),
    "Fat" numeric(4, 1),
    "SaturatedFat" numeric(4, 1),
	"Carbohydrate" numeric(4, 1),
	"Sugars" numeric(4, 1),
	"AddedSugars" numeric(4, 1),
	"Fiber" numeric(4, 1),
	"Protein" numeric(4, 1),
    "Sodium" smallint,
    "Cholesterol" smallint,
    "VitaminA" smallint,
    "VitaminC" smallint,
    "VitaminD" smallint,
    "Calcium" smallint,
    "Iron" smallint,
    "Potassium" smallint,
	"ProductSize" smallint NOT NULL DEFAULT 100,
    "ProductSizeIsOneUnit" boolean NOT NULL DEFAULT FALSE,
	"Price" numeric(6, 2),
	"Currency" character varying(3) COLLATE pg_catalog."default",
    "CreatedDate" timestamp with time zone NOT NULL,
    "ModifiedDate" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_CookingAssistant.Ingredients" PRIMARY KEY ("Id"),
    CONSTRAINT "UQ_CookingAssistant.Ingredients_Name_Type" UNIQUE ("Name", "Type"),
	CONSTRAINT "FK_CookingAssistant.Ingredients_CookingAssistant.Ingredients_ParentId" FOREIGN KEY ("ParentId")
    REFERENCES public."CookingAssistant.Ingredients" ("Id") MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE,
    CONSTRAINT "FK_CookingAssistant.Ingredients_CookingAssistant.IngredientCategories_CategoryId" FOREIGN KEY (FoodCategoryId)
    REFERENCES public."CookingAssistant.IngredientCategories" ("Id") MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE,
    CONSTRAINT "FK_CookingAssistant.Ingredients_AspNetUsers_UserId" FOREIGN KEY ("UserId")
    REFERENCES public."AspNetUsers" ("Id") MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE,
    CONSTRAINT "FK_CookingAssistant.Ingredients_ToDoAssistant.Tasks_TaskId" FOREIGN KEY ("TaskId")
    REFERENCES public."ToDoAssistant.Tasks" ("Id") MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE SET NULL
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE public."CookingAssistant.Ingredients"
    OWNER to personalassistant;

-- Index: IX_CookingAssistant.Ingredients_UserId

-- DROP INDEX public."IX_CookingAssistant.Ingredients_UserId";

CREATE INDEX "IX_CookingAssistant.Ingredients_UserId"
    ON public."CookingAssistant.Ingredients" USING btree
    ("UserId")
    TABLESPACE pg_default;

-- Index: IX_CookingAssistant.Ingredients_TaskId

-- DROP INDEX public."IX_CookingAssistant.Ingredients_TaskId";

CREATE INDEX "IX_CookingAssistant.Ingredients_TaskId"
    ON public."CookingAssistant.Ingredients" USING btree
    ("TaskId")
    TABLESPACE pg_default;
	
	
-- Seed public ingredients

INSERT INTO public."CookingAssistant.Ingredients"("ParentId", "CategoryId", "UserId", "Name", "MeasurementType", "CreatedDate", "ModifiedDate", "ServingSize", "ServingSizeIsOneUnit") VALUES

-- Grain
(NULL, 1, 1, 'flour', 1, NOW(), NOW(), 100, FALSE),
    (NULL, 1, 1, 'all_purpose_flour', 1, NOW(), NOW(), 100, FALSE),
(NULL, 1, 1, 'bread', NULL, NOW(), NOW(), 100, FALSE),
    (NULL, 1, 1, 'hamburger_bun', NULL, NOW(), NOW(), 100, FALSE),
    (NULL, 1, 1, 'bread_crumbs', NULL, NOW(), NOW(), 100, FALSE),
(NULL, 1, 1, 'pasta', NULL, NOW(), NOW(), 100, FALSE),
    (NULL, 1, 1, 'spaghetti', NULL, NOW(), NOW(), 100, FALSE),
    (NULL, 1, 1, 'macaroni', NULL, NOW(), NOW(), 100, FALSE),
    (NULL, 1, 1, 'fettuccine', NULL, NOW(), NOW(), 100, FALSE),
    (NULL, 1, 1, 'fusilli', NULL, NOW(), NOW(), 100, FALSE),
    (NULL, 1, 1, 'bow_tie_pasta', NULL, NOW(), NOW(), 100, FALSE),
    (NULL, 1, 1, 'gnocchi', NULL, NOW(), NOW(), 100, FALSE),
    (NULL, 1, 1, 'penne_pasta', NULL, NOW(), NOW(), 100, FALSE),
    (NULL, 1, 1, 'conchiglie_pasta', NULL, NOW(), NOW(), 100, FALSE),
    (NULL, 1, 1, 'farfalle_pasta', NULL, NOW(), NOW(), 100, FALSE),
    (NULL, 1, 1, 'noodles', NULL, NOW(), NOW(), 100, FALSE),
(NULL, 1, 1, 'corn_starch', NULL, NOW(), NOW(), 100, FALSE),
(NULL, 1, 1, 'cornmeal', NULL, NOW(), NOW(), 100, FALSE),
(NULL, 1, 1, 'muesli', NULL, NOW(), NOW(), 100, FALSE),
(NULL, 1, 1, 'oats', NULL, NOW(), NOW(), 100, FALSE),

-- Vegetables
(NULL, 2, 1, 'potatoes', NULL, NOW(), NOW(), 100, FALSE),
(NULL, 2, 1, 'carrots', NULL, NOW(), NOW(), 100, FALSE),
(NULL, 2, 1, 'tomatoes', NULL, NOW(), NOW(), 100, FALSE),
(NULL, 2, 1, 'onions', NULL, NOW(), NOW(), 100, FALSE),
(NULL, 2, 1, 'cucumbers', NULL, NOW(), NOW(), 100, FALSE),
(NULL, 2, 1, 'garlic', NULL, NOW(), NOW(), 100, FALSE),
    (NULL, 2, 1, 'garlic_clove', NULL, NOW(), NOW(), 100, FALSE),
(NULL, 2, 1, 'cabbage', NULL, NOW(), NOW(), 100, FALSE),
(NULL, 2, 1, 'spinach', NULL, NOW(), NOW(), 100, FALSE),
(NULL, 2, 1, 'broccoli', NULL, NOW(), NOW(), 100, FALSE),
(NULL, 2, 1, 'rice', NULL, NOW(), NOW(), 100, FALSE),
    (NULL, 2, 1, 'white_rice', NULL, NOW(), NOW(), 100, FALSE),
    (NULL, 2, 1, 'brown_rice', NULL, NOW(), NOW(), 100, FALSE),
(NULL, 2, 1, 'corn', NULL, NOW(), NOW(), 100, FALSE),
(NULL, 2, 1, 'lettuce', NULL, NOW(), NOW(), 100, FALSE),
(NULL, 2, 1, 'beans', NULL, NOW(), NOW(), 100, FALSE),
(NULL, 2, 1, 'mushrooms', NULL, NOW(), NOW(), 100, FALSE),
(NULL, 2, 1, 'peppers', NULL, NOW(), NOW(), 100, FALSE),
    (NULL, 2, 1, 'green_peppers', NULL, NOW(), NOW(), 100, FALSE),
    (NULL, 2, 1, 'red_peppers', NULL, NOW(), NOW(), 100, FALSE),
    (NULL, 2, 1, 'chili_peppers', NULL, NOW(), NOW(), 100, FALSE),
    (NULL, 2, 1, 'cayenne_peppers', NULL, NOW(), NOW(), 100, FALSE),
(NULL, 2, 1, 'pickles', NULL, NOW(), NOW(), 100, FALSE),
(NULL, 2, 1, 'cauliflower', NULL, NOW(), NOW(), 100, FALSE),
(NULL, 2, 1, 'asparagus', NULL, NOW(), NOW(), 100, FALSE),
(NULL, 2, 1, 'olives', NULL, NOW(), NOW(), 100, FALSE),
    (NULL, 2, 1, 'green_olives', NULL, NOW(), NOW(), 100, FALSE),
    (NULL, 2, 1, 'black_olives', NULL, NOW(), NOW(), 100, FALSE),
(NULL, 2, 1, 'celery', NULL, NOW(), NOW(), 100, FALSE),
(NULL, 2, 1, 'eggplant', NULL, NOW(), NOW(), 100, FALSE),
(NULL, 2, 1, 'parsnip', NULL, NOW(), NOW(), 100, FALSE),
(NULL, 2, 1, 'beetroot', NULL, NOW(), NOW(), 100, FALSE),
(NULL, 2, 1, 'zucchini', NULL, NOW(), NOW(), 100, FALSE),
(NULL, 2, 1, 'ginger', NULL, NOW(), NOW(), 100, FALSE),
(NULL, 2, 1, 'brussel_sprout', NULL, NOW(), NOW(), 100, FALSE),
(NULL, 2, 1, 'radish', NULL, NOW(), NOW(), 100, FALSE),
(NULL, 2, 1, 'turnip', NULL, NOW(), NOW(), 100, FALSE),

-- Fruit
(NULL, 3, 1, 'apples', NULL, NOW(), NOW(), 100, FALSE),
(NULL, 3, 1, 'bananas', NULL, NOW(), NOW(), 100, FALSE),
(NULL, 3, 1, 'pears', NULL, NOW(), NOW(), 100, FALSE),
(NULL, 3, 1, 'grapes', NULL, NOW(), NOW(), 100, FALSE),
(NULL, 3, 1, 'peaches', NULL, NOW(), NOW(), 100, FALSE),
(NULL, 3, 1, 'lemons', NULL, NOW(), NOW(), 100, FALSE),
(NULL, 3, 1, 'limes', NULL, NOW(), NOW(), 100, FALSE),
(NULL, 3, 1, 'mandarins', NULL, NOW(), NOW(), 100, FALSE),
(NULL, 3, 1, 'oranges', NULL, NOW(), NOW(), 100, FALSE),
(NULL, 3, 1, 'grapefruit', NULL, NOW(), NOW(), 100, FALSE),
(NULL, 3, 1, 'apricot', NULL, NOW(), NOW(), 100, FALSE),
(NULL, 3, 1, 'cherries', NULL, NOW(), NOW(), 100, FALSE),
(NULL, 3, 1, 'strawberries', NULL, NOW(), NOW(), 100, FALSE),
(NULL, 3, 1, 'cranberries', NULL, NOW(), NOW(), 100, FALSE),
(NULL, 3, 1, 'blueberries', NULL, NOW(), NOW(), 100, FALSE),
(NULL, 3, 1, 'raspberries', NULL, NOW(), NOW(), 100, FALSE),
(NULL, 3, 1, 'blackberries', NULL, NOW(), NOW(), 100, FALSE),
(NULL, 3, 1, 'watermelon', NULL, NOW(), NOW(), 100, FALSE),
(NULL, 3, 1, 'melon', NULL, NOW(), NOW(), 100, FALSE),
(NULL, 3, 1, 'pumpkin', NULL, NOW(), NOW(), 100, FALSE),
(NULL, 3, 1, 'pomegranate', NULL, NOW(), NOW(), 100, FALSE),
(NULL, 3, 1, 'coconut', NULL, NOW(), NOW(), 100, FALSE),
(NULL, 3, 1, 'avocado', NULL, NOW(), NOW(), 100, FALSE),
(NULL, 3, 1, 'pineapple', NULL, NOW(), NOW(), 100, FALSE),
(NULL, 3, 1, 'plums', NULL, NOW(), NOW(), 100, FALSE),
(NULL, 3, 1, 'kiwis', NULL, NOW(), NOW(), 100, FALSE),
(NULL, 3, 1, 'raisins', NULL, NOW(), NOW(), 100, FALSE),

-- Dairy
(NULL, 4, 1, 'butter', 2, NOW(), NOW(), 100, FALSE),
    (NULL, 4, 1, 'unsalted_butter', 2, NOW(), NOW(), 100, FALSE),
    (NULL, 4, 1, 'salted_butter', 2, NOW(), NOW(), 100, FALSE),
(NULL, 4, 1, 'margarine', 2, NOW(), NOW(), 100, FALSE),
(NULL, 4, 1, 'sour_cream', 2, NOW(), NOW(), 100, FALSE),
(NULL, 4, 1, 'heavy_cream', 2, NOW(), NOW(), 100, FALSE),
(NULL, 4, 1, 'milk', 2, NOW(), NOW(), 100, FALSE),
    (NULL, 4, 1, 'whole_milk', 2, NOW(), NOW(), 100, FALSE),
    (NULL, 4, 1, 'skim_milk', 2, NOW(), NOW(), 100, FALSE),
    (NULL, 4, 1, 'condensed_milk', 2, NOW(), NOW(), 100, FALSE),
(NULL, 4, 1, 'yogurt', 2, NOW(), NOW(), 100, FALSE),
-- Dairy/Cheese
(NULL, 10, 1, 'sheep_cheese', NULL, NOW(), NOW(), 100, FALSE),
(NULL, 10, 1, 'parmesan', NULL, NOW(), NOW(), 100, FALSE),
(NULL, 10, 1, 'cow_cheese', NULL, NOW(), NOW(), 100, FALSE),
(NULL, 10, 1, 'feta_cheese', NULL, NOW(), NOW(), 100, FALSE),
(NULL, 10, 1, 'cheddar_cheese', NULL, NOW(), NOW(), 100, FALSE),
(NULL, 10, 1, 'mozzarella_cheese', NULL, NOW(), NOW(), 100, FALSE),
(NULL, 10, 1, 'provolone_cheese', NULL, NOW(), NOW(), 100, FALSE),
(NULL, 10, 1, 'swiss_cheese', NULL, NOW(), NOW(), 100, FALSE),
(NULL, 10, 1, 'ricotta_cheese', NULL, NOW(), NOW(), 100, FALSE),

-- Protein
(NULL, 5, 1, 'chicken', 1, NOW(), NOW(), 100, FALSE),
    (NULL, 5, 1, 'chicken_breast', 1, NOW(), NOW(), 100, FALSE),
    (NULL, 5, 1, 'chicken_wings', 1, NOW(), NOW(), 100, FALSE),
(NULL, 5, 1, 'pork', 1, NOW(), NOW(), 100, FALSE),
    (NULL, NULL, 1, 'ham', 1, NOW(), NOW(), 100, FALSE),
    (NULL, NULL, 1, 'bacon', 1, NOW(), NOW(), 100, FALSE),
(NULL, 5, 1, 'beef', 1, NOW(), NOW(), 100, FALSE),
    (NULL, 5, 1, 'sirloin_steak', 1, NOW(), NOW(), 100, FALSE),
(NULL, 5, 1, 'lamb', 1, NOW(), NOW(), 100, FALSE),
(NULL, 5, 1, 'wiener', 1, NOW(), NOW(), 100, FALSE),
(NULL, 5, 1, 'sausage', 1, NOW(), NOW(), 100, FALSE),
(NULL, 5, 1, 'liver', 1, NOW(), NOW(), 100, FALSE),
(NULL, 5, 1, 'eggs', 0, NOW(), NOW(), 100, FALSE),
    (NULL, 5, 1, 'egg_whites', 0, NOW(), NOW(), 100, FALSE),
    (NULL, 5, 1, 'egg_yolks', 0, NOW(), NOW(), 100, FALSE),
(NULL, 5, 1, 'tofu', 1, NOW(), NOW(), 100, FALSE),
-- Protein/Seafood
(NULL, 11, 1, 'shrimp', 1, NOW(), NOW(), 100, FALSE),
(NULL, 11, 1, 'hake', 1, NOW(), NOW(), 100, FALSE),
(NULL, 11, 1, 'sardines', 1, NOW(), NOW(), 100, FALSE),
(NULL, 11, 1, 'tuna', 1, NOW(), NOW(), 100, FALSE),

-- Nuts/legumes
(NULL, 6, 1, 'almonds', 1, NOW(), NOW(), 100, FALSE),
(NULL, 6, 1, 'cashew_nuts', 1, NOW(), NOW(), 100, FALSE),
(NULL, 6, 1, 'hazelnuts', 1, NOW(), NOW(), 100, FALSE),
(NULL, 6, 1, 'pistachios', 1, NOW(), NOW(), 100, FALSE),
(NULL, 6, 1, 'walnuts', 1, NOW(), NOW(), 100, FALSE),
(NULL, 6, 1, 'peanuts', 1, NOW(), NOW(), 100, FALSE),
(NULL, 6, 1, 'sunflower_seeds', 1, NOW(), NOW(), 100, FALSE),
(NULL, 6, 1, 'sesame_seeds', 1, NOW(), NOW(), 100, FALSE),
(NULL, 6, 1, 'chia_seeds', 1, NOW(), NOW(), 100, FALSE),
(NULL, 6, 1, 'peas', NULL, NOW(), NOW(), 100, FALSE),
    (NULL, 6, 1, 'green_peas', NULL, NOW(), NOW(), 100, FALSE),
(NULL, 6, 1, 'lentils', 1, NOW(), NOW(), 100, FALSE),
(NULL, 6, 1, 'chickpeas', 1, NOW(), NOW(), 100, FALSE),
(NULL, 6, 1, 'lima_beans', 1, NOW(), NOW(), 100, FALSE),
(NULL, 6, 1, 'cumin', NULL, NOW(), NOW(), 100, FALSE),

-- Seasoning
(NULL, 7, 1, 'salt', NULL, NOW(), NOW(), 100, FALSE),
    (NULL, 7, 1, 'sea_salt', NULL, NOW(), NOW(), 100, FALSE),
    (NULL, 7, 1, 'himalayan_salt', NULL, NOW(), NOW(), 100, FALSE),
    (NULL, 7, 1, 'kosher_salt', NULL, NOW(), NOW(), 100, FALSE),
(NULL, 7, 1, 'sugar', NULL, NOW(), NOW(), 100, FALSE),
    (NULL, 7, 1, 'brown_sugar', NULL, NOW(), NOW(), 100, FALSE),
(NULL, 7, 1, 'black_pepper', NULL, NOW(), NOW(), 100, FALSE),
(NULL, 7, 1, 'oregano', NULL, NOW(), NOW(), 100, FALSE),
(NULL, 7, 1, 'basil', NULL, NOW(), NOW(), 100, FALSE),
(NULL, 7, 1, 'rosemary', NULL, NOW(), NOW(), 100, FALSE),
(NULL, 7, 1, 'cayenne_pepper_powder', NULL, NOW(), NOW(), 100, FALSE),
(NULL, 7, 1, 'nutmeg', NULL, NOW(), NOW(), 100, FALSE),
(NULL, 7, 1, 'paprika', NULL, NOW(), NOW(), 100, FALSE),
(NULL, 7, 1, 'thyme', NULL, NOW(), NOW(), 100, FALSE),
(NULL, 7, 1, 'cinnamon', NULL, NOW(), NOW(), 100, FALSE),
(NULL, 7, 1, 'garlic_powder', NULL, NOW(), NOW(), 100, FALSE),
(NULL, 7, 1, 'onion_powder', NULL, NOW(), NOW(), 100, FALSE),
(NULL, 7, 1, 'chili_powder', NULL, NOW(), NOW(), 100, FALSE),
(NULL, 7, 1, 'curry_powder', NULL, NOW(), NOW(), 100, FALSE),
(NULL, 7, 1, 'garam_masala', NULL, NOW(), NOW(), 100, FALSE),
(NULL, 7, 1, 'turmeric', NULL, NOW(), NOW(), 100, FALSE),
(NULL, 7, 1, 'italian_seasoning', NULL, NOW(), NOW(), 100, FALSE),

-- Alcohol
(NULL, 8, 1, 'beer', NULL, NOW(), NOW(), 100, FALSE),
(NULL, 8, 1, 'wine', NULL, NOW(), NOW(), 100, FALSE),
    (NULL, 8, 1, 'white_wine', NULL, NOW(), NOW(), 100, FALSE),
        (NULL, 8, 1, 'dry_white_wine', NULL, NOW(), NOW(), 100, FALSE),
    (NULL, 8, 1, 'red_wine', NULL, NOW(), NOW(), 100, FALSE),
(NULL, 8, 1, 'rum', NULL, NOW(), NOW(), 100, FALSE),

-- Misc
(NULL, NULL, 1, 'water', NULL, NOW(), NOW(), 100, FALSE),
(NULL, NULL, 1, 'ketchup', NULL, NOW(), NOW(), 100, FALSE),
(NULL, NULL, 1, 'mayonnaise', NULL, NOW(), NOW(), 100, FALSE),
(NULL, NULL, 1, 'mustard', NULL, NOW(), NOW(), 100, FALSE),
(NULL, NULL, 1, 'sweet_pickle_relish', NULL, NOW(), NOW(), 100, FALSE),
(NULL, NULL, 1, 'sunflower_oil', NULL, NOW(), NOW(), 100, FALSE),
(NULL, NULL, 1, 'oil', NULL, NOW(), NOW(), 100, FALSE),
    (NULL, NULL, 1, 'vegetable_oil', NULL, NOW(), NOW(), 100, FALSE),
    (NULL, NULL, 1, 'olive_oil', NULL, NOW(), NOW(), 100, FALSE),
    (NULL, NULL, 1, 'canola_oil', NULL, NOW(), NOW(), 100, FALSE),
(NULL, NULL, 1, 'baking_powder', NULL, NOW(), NOW(), 100, FALSE),
(NULL, NULL, 1, 'baking_soda', NULL, NOW(), NOW(), 100, FALSE),
(NULL, NULL, 1, 'barbeque_sauce', NULL, NOW(), NOW(), 100, FALSE),
(NULL, NULL, 1, 'worcestershire_sauce', NULL, NOW(), NOW(), 100, FALSE),
(NULL, NULL, 1, 'soy_sauce', NULL, NOW(), NOW(), 100, FALSE),
(NULL, NULL, 1, 'curry_sauce', NULL, NOW(), NOW(), 100, FALSE),
(NULL, NULL, 1, 'salsa', NULL, NOW(), NOW(), 100, FALSE),
(NULL, NULL, 1, 'vinegar', NULL, NOW(), NOW(), 100, FALSE),
    (NULL, NULL, 1, 'white_vinegar', NULL, NOW(), NOW(), 100, FALSE),
    (NULL, NULL, 1, 'red_vinegar', NULL, NOW(), NOW(), 100, FALSE),
    (NULL, NULL, 1, 'balsamic_vinegar', NULL, NOW(), NOW(), 100, FALSE),
    (NULL, NULL, 1, 'apple_cider_vinegar', NULL, NOW(), NOW(), 100, FALSE),
    (NULL, NULL, 1, 'rice_vinegar', NULL, NOW(), NOW(), 100, FALSE),
(NULL, NULL, 1, 'ranch_dressing', NULL, NOW(), NOW(), 100, FALSE),
(NULL, NULL, 1, 'tartar_sauce', NULL, NOW(), NOW(), 100, FALSE),
(NULL, NULL, 1, 'hoisin_sauce', NULL, NOW(), NOW(), 100, FALSE),
(NULL, NULL, 1, 'tomato_sauce', NULL, NOW(), NOW(), 100, FALSE),
(NULL, NULL, 1, 'tomato_paste', NULL, NOW(), NOW(), 100, FALSE),
(NULL, NULL, 1, 'stewed_tomatoes', NULL, NOW(), NOW(), 100, FALSE),
(NULL, NULL, 1, 'chicken_broth', NULL, NOW(), NOW(), 100, FALSE),
(NULL, NULL, 1, 'honey', NULL, NOW(), NOW(), 100, FALSE),
(NULL, NULL, 1, 'maple_syrup', NULL, NOW(), NOW(), 100, FALSE),
(NULL, NULL, 1, 'mushroom_soup', NULL, NOW(), NOW(), 100, FALSE),
(NULL, NULL, 1, 'condensed_chicken_soup', NULL, NOW(), NOW(), 100, FALSE),
(NULL, NULL, 1, 'chicken_bouillon_cube', NULL, NOW(), NOW(), 100, FALSE),
(NULL, NULL, 1, 'vanilla_extract', NULL, NOW(), NOW(), 100, FALSE),
(NULL, NULL, 1, 'peanut_butter', NULL, NOW(), NOW(), 100, FALSE),
(NULL, NULL, 1, 'biscuits', NULL, NOW(), NOW(), 100, FALSE),
(NULL, NULL, 1, 'powdered_sugar', NULL, NOW(), NOW(), 100, FALSE),
(NULL, NULL, 1, 'coffee', NULL, NOW(), NOW(), 100, FALSE),
(NULL, NULL, 1, 'cocoa_powder', NULL, NOW(), NOW(), 100, FALSE),
(NULL, NULL, 1, 'active_dry_yeast', NULL, NOW(), NOW(), 100, FALSE),
(NULL, NULL, 1, 'baking_chocolate', NULL, NOW(), NOW(), 100, FALSE),
(NULL, NULL, 1, 'chocolate_chips', NULL, NOW(), NOW(), 100, FALSE);