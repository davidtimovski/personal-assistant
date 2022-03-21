-- Table: public."Accountant.Categories"

-- DROP TABLE public."Accountant.Categories";

CREATE TABLE public."Accountant.Categories"
(
    "Id" serial NOT NULL,
	"ParentId" integer,
	"UserId" integer NOT NULL,
    "Name" character varying(30) NOT NULL COLLATE pg_catalog."default",
	"Type" smallint NOT NULL,
	"GenerateUpcomingExpense" boolean NOT NULL DEFAULT FALSE,
	"IsTax" boolean NOT NULL DEFAULT FALSE,
    "CreatedDate" timestamp with time zone NOT NULL,
    "ModifiedDate" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_Accountant.Categories" PRIMARY KEY ("Id"),
	CONSTRAINT "FK_Accountant.Categories_Accountant.Categories_ParentId" FOREIGN KEY ("ParentId")
    REFERENCES public."Accountant.Categories" ("Id") MATCH SIMPLE
	ON UPDATE NO ACTION
    ON DELETE SET NULL,
	CONSTRAINT "FK_Accountant.Categories_AspNetUsers_UserId" FOREIGN KEY ("UserId")
    REFERENCES public."AspNetUsers" ("Id") MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE public."Accountant.Categories"
    OWNER to personalassistant;
	
-- Index: IX_Accountant.Categories_ParentId

-- DROP INDEX public."IX_Accountant.Categories_ParentId";

CREATE INDEX "IX_Accountant.Categories_ParentId"
    ON public."Accountant.Categories" USING btree
    ("ParentId")
    TABLESPACE pg_default;

-- Index: IX_Accountant.Categories_UserId

-- DROP INDEX public."IX_Accountant.Categories_UserId";

CREATE INDEX "IX_Accountant.Categories_UserId"
    ON public."Accountant.Categories" USING btree
    ("UserId")
    TABLESPACE pg_default;
