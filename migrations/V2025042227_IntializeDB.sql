CREATE TABLE roles (
    role_id SERIAL PRIMARY KEY,
    role_name VARCHAR(50) NOT NULL
);

CREATE TABLE users (
    google_id VARCHAR(100) PRIMARY KEY,
    email VARCHAR(256) NOT NULL,
    first_name VARCHAR(50) NOT NULL,
    last_name VARCHAR(50) NOT NULL,
    role_id INT NOT NULL REFERENCES roles(role_id),
    is_active BOOLEAN NOT NULL
);

CREATE TABLE galaxies (
    galaxy_id SERIAL PRIMARY KEY,
    galaxy_name VARCHAR(100) NOT NULL,
    is_active BOOLEAN NOT NULL
);

CREATE TABLE star_systems (
    system_id SERIAL PRIMARY KEY,
    system_name VARCHAR(100) NOT NULL,
    galaxy_id INT NOT NULL REFERENCES galaxies(galaxy_id),
    is_active BOOLEAN NOT NULL
);

CREATE TABLE destinations (
    destination_id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    system_id INT NOT NULL REFERENCES star_systems(system_id),
    distance_from_earth INT NOT NULL,
    is_active BOOLEAN NOT NULL
);

CREATE TABLE ship_models (
    model_id SERIAL PRIMARY KEY,
    model_name VARCHAR(100) NOT NULL,
    capacity INT NOT NULL,
    cruise_speed_kmph INT NOT NULL
);

CREATE TABLE spaceships (
    spaceship_id SERIAL PRIMARY KEY,
    model_id INT NOT NULL REFERENCES ship_models(model_id),
    is_active BOOLEAN NOT NULL
);

CREATE TABLE cruise_statuses (
    cruise_status_id SERIAL PRIMARY KEY,
    status_name VARCHAR(50) NOT NULL
);

CREATE TABLE cruises (
    cruise_id SERIAL PRIMARY KEY,
    spaceship_id INT NOT NULL REFERENCES spaceships(spaceship_id),
    departure_destination_id INT NOT NULL REFERENCES destinations(destination_id),
    arrival_destination_id INT NOT NULL REFERENCES destinations(destination_id),
    local_departure_time TIMESTAMP NOT NULL,
    duration_minutes INT NOT NULL,
    cruise_seat_price NUMERIC NOT NULL,
    cruise_status_id INT NOT NULL REFERENCES cruise_statuses(cruise_status_id),
    created_by_google_id VARCHAR(100) NOT NULL REFERENCES users(google_id)
);

CREATE TABLE booking_statuses (
    booking_status_id SERIAL PRIMARY KEY,
    status_name VARCHAR(50) NOT NULL
);

CREATE TABLE bookings (
    booking_id SERIAL PRIMARY KEY,
    google_id VARCHAR(100) NOT NULL REFERENCES users(google_id),
    cruise_id INT NOT NULL REFERENCES cruises(cruise_id),
    seat_number INT NOT NULL,
    booking_date TIMESTAMP NOT NULL,
    booking_expiration TIMESTAMP NOT NULL,
    booking_status_id INT NOT NULL REFERENCES booking_statuses(booking_status_id)
);

CREATE TABLE booking_history (
    history_id SERIAL PRIMARY KEY,
    booking_id INT NOT NULL REFERENCES bookings(booking_id),
    previous_booking_status_id INT NOT NULL REFERENCES booking_statuses(booking_status_id),
    new_booking_status_id INT NOT NULL REFERENCES booking_statuses(booking_status_id),
    changed_at TIMESTAMP NOT NULL
);
