-- Table: public."Accountant.DeletedEntities"

-- DROP TABLE public."Accountant.DeletedEntities";

CREATE TABLE public."Accountant.DeletedEntities"
(
	"UserId" integer NOT NULL,
	"EntityType" smallint NOT NULL,
	"EntityId" integer NOT NULL,
    "DeletedDate" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_Accountant.DeletedEntities" PRIMARY KEY ("UserId", "EntityType", "EntityId"),
	CONSTRAINT "FK_Accountant.DeletedEntities_AspNetUsers_UserId" FOREIGN KEY ("UserId")
    REFERENCES public."AspNetUsers" ("Id") MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE public."Accountant.DeletedEntities"
    OWNER to personalassistant;

-- Index: IX_Accountant.DeletedEntities_UserIdEntityType

-- DROP INDEX public."IX_Accountant.DeletedEntities_UserIdEntityType";

CREATE INDEX "IX_Accountant.DeletedEntities_UserIdEntityType"
    ON public."Accountant.DeletedEntities" USING btree
    ("UserId", "EntityType")
    TABLESPACE pg_default;
