-- Vicinia Database Initialization Script

-- Create extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "postgis";

-- Users table
CREATE TABLE IF NOT EXISTS users (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    email VARCHAR(255) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    first_name VARCHAR(100),
    last_name VARCHAR(100),
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    is_active BOOLEAN DEFAULT TRUE
);

-- Search history table
CREATE TABLE IF NOT EXISTS search_history (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID REFERENCES users(id) ON DELETE CASCADE,
    address VARCHAR(500) NOT NULL,
    latitude DOUBLE PRECISION NOT NULL,
    longitude DOUBLE PRECISION NOT NULL,
    transportation_mode VARCHAR(20) NOT NULL, -- 'car' or 'walking'
    overall_score DOUBLE PRECISION NOT NULL,
    search_date TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    poi_count INTEGER DEFAULT 0
);

-- Logs table
CREATE TABLE IF NOT EXISTS logs (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    service_name VARCHAR(100) NOT NULL,
    log_level VARCHAR(20) NOT NULL, -- 'Information', 'Warning', 'Error', 'Fatal'
    message TEXT NOT NULL,
    exception TEXT,
    timestamp TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    user_id UUID REFERENCES users(id) ON DELETE SET NULL,
    request_id VARCHAR(100),
    additional_data JSONB
);

-- Create indexes for better performance
CREATE INDEX IF NOT EXISTS idx_users_email ON users(email);
CREATE INDEX IF NOT EXISTS idx_search_history_user_id ON search_history(user_id);
CREATE INDEX IF NOT EXISTS idx_search_history_date ON search_history(search_date);
CREATE INDEX IF NOT EXISTS idx_logs_service_name ON logs(service_name);
CREATE INDEX IF NOT EXISTS idx_logs_timestamp ON logs(timestamp);
CREATE INDEX IF NOT EXISTS idx_logs_level ON logs(log_level);

-- Create function to update updated_at timestamp
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ language 'plpgsql';

-- Create trigger for users table
CREATE TRIGGER update_users_updated_at 
    BEFORE UPDATE ON users 
    FOR EACH ROW 
    EXECUTE FUNCTION update_updated_at_column();

-- Insert some sample data for testing
INSERT INTO users (email, password_hash, first_name, last_name) VALUES
    ('test@vicinia.com', '$2a$10$dummy.hash.for.testing', 'Test', 'User')
ON CONFLICT (email) DO NOTHING;

-- Create a view for search statistics
CREATE OR REPLACE VIEW search_statistics AS
SELECT 
    u.email,
    COUNT(sh.id) as total_searches,
    AVG(sh.overall_score) as average_score,
    MAX(sh.search_date) as last_search_date
FROM users u
LEFT JOIN search_history sh ON u.id = sh.user_id
GROUP BY u.id, u.email;

-- Grant permissions
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO vicinia_user;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO vicinia_user;
GRANT EXECUTE ON ALL FUNCTIONS IN SCHEMA public TO vicinia_user;

-- Initialize Vicinia databases
-- This script creates separate databases for each service

-- Create database for User Service
CREATE DATABASE vicinia_users;

-- Create database for History Service
CREATE DATABASE vicinia_history;

-- Create database for Logging Service
CREATE DATABASE vicinia_logging;

-- Grant permissions to vicinia_user
GRANT ALL PRIVILEGES ON DATABASE vicinia_users TO vicinia_user;
GRANT ALL PRIVILEGES ON DATABASE vicinia_history TO vicinia_user;
GRANT ALL PRIVILEGES ON DATABASE vicinia_logging TO vicinia_user;

-- Connect to each database and grant schema permissions
\c vicinia_users;
GRANT ALL ON SCHEMA public TO vicinia_user;

\c vicinia_history;
GRANT ALL ON SCHEMA public TO vicinia_user;

\c vicinia_logging;
GRANT ALL ON SCHEMA public TO vicinia_user;

-- Return to main database
\c vicinia; 