CREATE TABLE public.users
(
    id serial NOT NULL,
	email character varying(256) COLLATE pg_catalog."default" NOT NULL,
    name character varying(30) COLLATE pg_catalog."default" NOT NULL,
	country character varying(2) COLLATE pg_catalog."default",
    language character varying(5) COLLATE pg_catalog."default" NOT NULL,
	culture character varying(10) COLLATE pg_catalog."default" NOT NULL,
	image_uri character varying(255) COLLATE pg_catalog."default" NOT NULL,
    todo_notifications_enabled boolean NOT NULL DEFAULT FALSE,
    chef_notifications_enabled boolean NOT NULL DEFAULT FALSE,
    imperial_system boolean NOT NULL DEFAULT FALSE,
	modified_date timestamp with time zone NOT NULL,
    CONSTRAINT pk_users PRIMARY KEY (id)
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE public.users
    OWNER to personalassistant;
