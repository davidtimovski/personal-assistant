-- Table: public."CookingAssistant.Ingredients"

-- DROP TABLE public."CookingAssistant.Ingredients";

CREATE TABLE public."CookingAssistant.Ingredients"
(
    "Id" serial NOT NULL,
	"ParentId" integer,
    "UserId" integer NOT NULL,
	"CategoryId" integer,
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
	CONSTRAINT "FK_CookingAssistant.Ingredients_AspNetUsers_UserId" FOREIGN KEY ("UserId")
    REFERENCES public."AspNetUsers" ("Id") MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE,
    CONSTRAINT "FK_CookingAssistant.Ingredients_CookingAssistant.IngredientCategories_CategoryId" FOREIGN KEY ("CategoryId")
    REFERENCES public."CookingAssistant.IngredientCategories" ("Id") MATCH SIMPLE
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
