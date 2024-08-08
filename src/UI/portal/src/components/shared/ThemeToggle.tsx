import { useState, useEffect } from 'react';

const ThemeToggle = () => {
    const [isDarkMode, setIsDarkMode] = useState(false);

    useEffect(() => {
        const storedTheme = localStorage.getItem('theme');
        if (storedTheme === 'dark') {
            setIsDarkMode(true);
            applyTheme('dark');
        } else {
            applyTheme('light');
        }
    }, []);

    const applyTheme = (theme: 'light' | 'dark') => {
        const bootstrapCSS = document.getElementById('bootstrap-css') as HTMLAnchorElement;
        const appCSS = document.getElementById('app-css') as HTMLAnchorElement;

        if (bootstrapCSS && appCSS) {
            if (theme === 'dark') {
                bootstrapCSS.href = bootstrapCSS.href?.replace('bootstrap.min.css', 'bootstrap-dark.min.css');
                appCSS.href = appCSS.href?.replace('app.min.css', 'app-dark.min.css');
            } else {
                bootstrapCSS.href = bootstrapCSS.href?.replace('bootstrap-dark.min.css', 'bootstrap.min.css');
                appCSS.href = appCSS.href?.replace('app-dark.min.css', 'app.min.css');
            }
        }
    };

    const toggleTheme = () => {
        const newTheme = isDarkMode ? 'light' : 'dark';
        setIsDarkMode(!isDarkMode);
        applyTheme(newTheme);
        localStorage.setItem('theme', newTheme);
    };

    return (
        <button onClick={toggleTheme} className='btn nav-link dropdown-toggle arrow-none nav-icon'>
            {isDarkMode ? (
                "🌙"
            ) : (
                "🔆"
            )}
        </button>
    );
};

export default ThemeToggle;