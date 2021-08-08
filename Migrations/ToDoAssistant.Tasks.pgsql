-- Table: public."ToDoAssistant.Tasks"

-- DROP TABLE public."ToDoAssistant.Tasks";

CREATE TABLE public."ToDoAssistant.Tasks"
(
    "Id" serial NOT NULL,
    "ListId" integer NOT NULL,
    "Name" character varying(50) NOT NULL COLLATE pg_catalog."default",
    "IsCompleted" boolean NOT NULL DEFAULT FALSE,
    "IsOneTime" boolean NOT NULL DEFAULT FALSE,
    "IsHighPriority" boolean NOT NULL DEFAULT FALSE,
    "PrivateToUserId" integer,
    "AssignedToUserId" integer,
    "Order" smallint NOT NULL,
    "CreatedDate" timestamp without time zone NOT NULL,
    "ModifiedDate" timestamp without time zone NOT NULL,
    CONSTRAINT "PK_ToDoAssistant.Tasks" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_ToDoAssistant.Tasks_ToDoAssistant.Lists_ListId" FOREIGN KEY ("ListId")
    REFERENCES public."ToDoAssistant.Lists" ("Id") MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE,
    CONSTRAINT "FK_ToDoAssistant.Tasks_AspNetUsers_PrivateToUserId" FOREIGN KEY ("PrivateToUserId")
    REFERENCES public."AspNetUsers" ("Id") MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE,
    CONSTRAINT "FK_ToDoAssistant.Tasks_AspNetUsers_AssignedToUserId" FOREIGN KEY ("AssignedToUserId")
    REFERENCES public."AspNetUsers" ("Id") MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE public."ToDoAssistant.Tasks"
    OWNER to personalassistant;

-- Index: IX_ToDoAssistant.Tasks_ListId

-- DROP INDEX public."IX_ToDoAssistant.Tasks_ListId";

CREATE INDEX "IX_ToDoAssistant.Tasks_ListId"
    ON public."ToDoAssistant.Tasks" USING btree
    ("ListId")
    TABLESPACE pg_default;