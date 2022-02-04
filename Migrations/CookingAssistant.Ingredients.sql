-- Table: public."CookingAssistant.Ingredients"

-- DROP TABLE public."CookingAssistant.Ingredients";

CREATE TABLE public."CookingAssistant.Ingredients"
(
    "Id" serial NOT NULL,
    "UserId" integer NOT NULL,
    "TaskId" integer,
    "Name" character varying(50) COLLATE pg_catalog."default",
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
	