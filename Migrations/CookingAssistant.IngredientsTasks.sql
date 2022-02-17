-- Table: public."CookingAssistant.IngredientsTasks"

-- DROP TABLE public."CookingAssistant.IngredientsTasks";

CREATE TABLE IF NOT EXISTS public."CookingAssistant.IngredientsTasks"
(
    "IngredientId" integer NOT NULL,
    "TaskId" integer NOT NULL,
	"UserId" integer NOT NULL,
    "CreatedDate" timestamp with time zone NOT NULL,
    "ModifiedDate" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_CA.IngredientsTasks" PRIMARY KEY ("IngredientId", "TaskId"),
	CONSTRAINT "UQ_CA.IngredientsTasks_IngredientId_UserId" UNIQUE ("IngredientId", "UserId"),
	CONSTRAINT "FK_CA.IngredientsTasks_CA.Ingredients_IngredientId" FOREIGN KEY ("IngredientId")
        REFERENCES public."CookingAssistant.Ingredients" ("Id") MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE CASCADE,
    CONSTRAINT "FK_CA.IngredientsTasks_TDA.Tasks_TaskId" FOREIGN KEY ("TaskId")
        REFERENCES public."ToDoAssistant.Tasks" ("Id") MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE CASCADE,
	CONSTRAINT "FK_CA.IngredientsTasks_AspNetUsers_UserId" FOREIGN KEY ("UserId")
        REFERENCES public."AspNetUsers" ("Id") MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE CASCADE
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE public."CookingAssistant.IngredientsTasks"
    OWNER to personalassistant;
	
-- Index: IX_CA.IngredientsTasks_IngredientId

-- DROP INDEX public."IX_CA.IngredientsTasks_IngredientId";

CREATE INDEX "IX_CA.IngredientsTasks_IngredientId"
    ON public."CookingAssistant.IngredientsTasks" USING btree
    ("IngredientId")
    TABLESPACE pg_default;

-- Index: IX_CookingAssistant.IngredientsTasks_TaskId

-- DROP INDEX public."IX_CA.IngredientsTasks_TaskId";

CREATE INDEX "IX_CA.IngredientsTasks_TaskId"
    ON public."CookingAssistant.IngredientsTasks" USING btree
    ("TaskId")
    TABLESPACE pg_default;

-- Index: IX_CookingAssistant.IngredientsTasks_UserId

-- DROP INDEX public."IX_CA.IngredientsTasks_UserId";

CREATE INDEX "IX_CA.IngredientsTasks_UserId"
    ON public."CookingAssistant.IngredientsTasks" USING btree
    ("UserId")
    TABLESPACE pg_default;
