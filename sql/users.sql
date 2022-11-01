CREATE TABLE public.users
(
    id serial NOT NULL,
	email character varying(256) COLLATE pg_catalog."default" NOT NULL,
    name character varying(30) COLLATE pg_catalog."default" NOT NULL,
    language character varying(5) COLLATE pg_catalog."default" NOT NULL,
	culture character varying(10) COLLATE pg_catalog."default" NOT NULL,
	image_uri character varying(255) NOT NULL COLLATE pg_catalog."default",
    todo_notifications_enabled boolean NOT NULL DEFAULT FALSE,
    cooking_notifications_enabled boolean NOT NULL DEFAULT FALSE,
    imperial_system boolean NOT NULL DEFAULT FALSE,
    CONSTRAINT pk_users PRIMARY KEY (id)
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE public.users
    OWNER to personalassistant;
