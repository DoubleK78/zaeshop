import React, { useCallback, useEffect, useMemo } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { useNavigate } from 'react-router-dom';
import { StoreState } from '../../store';
import { signOut } from '../../store/thunks/authThunk';
import { useTranslation } from 'react-i18next';

const Navbar: React.FC = () => {
    const auth = useSelector((state: StoreState) => state.auth);
    const dispatch = useDispatch();
    const navigate = useNavigate();

    // i18n
    const { t, i18n } = useTranslation();
    const changeLanguage = useCallback((language: string) => {
        i18n.changeLanguage(language);
    }, [i18n]);

    useEffect(() => {
        if (!auth.isAuthenticate) {
            navigate("/login");
        }
    }, [navigate, auth.isAuthenticate]);

    useEffect(() => {
        const isCollapsedMenu = (() => {
            const storedValue = localStorage.getItem('isCollapsedMenu');

            if (storedValue === null) {
                return false;
            }

            try {
                return JSON.parse(storedValue) as boolean;
            } catch {
                return false;
            }
        })();

        if (isCollapsedMenu === true) {
            toggleCollapsedMenu();
        }
    }, []);

    const imageLocation = useMemo(() => {
        if (i18n.language === 'en') {
            changeLanguage('en');
            return process.env.PUBLIC_URL + '/assets/images/flags/us_flag.jpg';
        }

        changeLanguage('vi');
        return process.env.PUBLIC_URL + '/assets/images/flags/vietnam-flag.png';
    }, [i18n.language, changeLanguage]);

    const logout = async () => {
        await signOut()(dispatch);
        navigate("/login");
    }

    const toggleCollapsedMenu = () => {
        const bodyElement = document.getElementById('body');
        if (bodyElement) {
            if (bodyElement.classList.toggle('enlarge-menu')) {
                localStorage.setItem('isCollapsedMenu', JSON.stringify(true));
            }
            else {
                localStorage.setItem('isCollapsedMenu', JSON.stringify(false));
            }
        }
    }

    return (
        <div className="topbar">
            {/* Navbar */}
            <nav className="navbar-custom" id="navbar-custom">
                <ul className="list-unstyled topbar-nav float-end mb-0">
                    <li className="dropdown">
                        <a
                            className="nav-link dropdown-toggle arrow-none nav-icon"
                            data-bs-toggle="dropdown"
                            role="button"
                            aria-haspopup="false"
                            aria-expanded="false"
                        >
                            <img
                                src={imageLocation}
                                alt=""
                                className="thumb-xxs rounded-circle"
                            />
                        </a>
                        <div className="dropdown-menu">
                            <a className="dropdown-item"
                                onClick={() => changeLanguage("en")}>
                                <img
                                    src={process.env.PUBLIC_URL + "/assets/images/flags/us_flag.jpg"}
                                    alt=""
                                    height={15}
                                    className="me-2"
                                />
                                {t('navbar.english')}
                            </a>
                            <a className="dropdown-item"
                                onClick={() => changeLanguage("vi")}>
                                <img
                                    src={process.env.PUBLIC_URL + "/assets/images/flags/vietnam-flag.png"}
                                    alt=""
                                    height={25}
                                    className="me-2"
                                />
                                {t('navbar.vietnamese')}
                            </a>
                        </div>
                    </li>
                    {/*end topbar-language*/}
                    <li className="dropdown notification-list">
                        <a
                            className="nav-link dropdown-toggle arrow-none nav-icon"
                            data-bs-toggle="dropdown"
                            href="crm-contacts.html#"
                            role="button"
                            aria-haspopup="false"
                            aria-expanded="false"
                        >
                            <i className="ti ti-bell" />
                            <span className="alert-badge" />
                        </a>
                        <div className="dropdown-menu dropdown-menu-end dropdown-lg pt-0">
                            <h6 className="dropdown-item-text font-15 m-0 py-3 border-bottom d-flex justify-content-between align-items-center">
                                {t('navbar.title')}{" "}
                                <span className="badge bg-soft-primary badge-pill">2</span>
                            </h6>
                            <div className="notification-menu" data-simplebar="">
                                {/* item*/}
                                <a href="crm-contacts.html#" className="dropdown-item py-3">
                                    <small className="float-end text-muted ps-2">2 min ago</small>
                                    <div className="media">
                                        <div className="avatar-md bg-soft-primary">
                                            <i className="ti ti-chart-arcs" />
                                        </div>
                                        <div className="media-body align-self-center ms-2 text-truncate">
                                            <h6 className="my-0 fw-normal text-dark">
                                                Your order is placed
                                            </h6>
                                            <small className="text-muted mb-0">
                                                Dummy text of the printing and industry.
                                            </small>
                                        </div>
                                        {/*end media-body*/}
                                    </div>
                                    {/*end media*/}
                                </a>
                                {/*end-item*/}
                                {/* item*/}
                                <a href="crm-contacts.html#" className="dropdown-item py-3">
                                    <small className="float-end text-muted ps-2">10 min ago</small>
                                    <div className="media">
                                        <div className="avatar-md bg-soft-primary">
                                            <i className="ti ti-device-computer-camera" />
                                        </div>
                                        <div className="media-body align-self-center ms-2 text-truncate">
                                            <h6 className="my-0 fw-normal text-dark">
                                                Meeting with designers
                                            </h6>
                                            <small className="text-muted mb-0">
                                                It is a long established fact that a reader.
                                            </small>
                                        </div>
                                        {/*end media-body*/}
                                    </div>
                                    {/*end media*/}
                                </a>
                                {/*end-item*/}
                                {/* item*/}
                                <a href="crm-contacts.html#" className="dropdown-item py-3">
                                    <small className="float-end text-muted ps-2">40 min ago</small>
                                    <div className="media">
                                        <div className="avatar-md bg-soft-primary">
                                            <i className="ti ti-diamond" />
                                        </div>
                                        <div className="media-body align-self-center ms-2 text-truncate">
                                            <h6 className="my-0 fw-normal text-dark">
                                                UX 3 Task complete.
                                            </h6>
                                            <small className="text-muted mb-0">
                                                Dummy text of the printing.
                                            </small>
                                        </div>
                                        {/*end media-body*/}
                                    </div>
                                    {/*end media*/}
                                </a>
                                {/*end-item*/}
                                {/* item*/}
                                <a href="crm-contacts.html#" className="dropdown-item py-3">
                                    <small className="float-end text-muted ps-2">1 hr ago</small>
                                    <div className="media">
                                        <div className="avatar-md bg-soft-primary">
                                            <i className="ti ti-drone" />
                                        </div>
                                        <div className="media-body align-self-center ms-2 text-truncate">
                                            <h6 className="my-0 fw-normal text-dark">
                                                Your order is placed
                                            </h6>
                                            <small className="text-muted mb-0">
                                                It is a long established fact that a reader.
                                            </small>
                                        </div>
                                        {/*end media-body*/}
                                    </div>
                                    {/*end media*/}
                                </a>
                                {/*end-item*/}
                                {/* item*/}
                                <a href="crm-contacts.html#" className="dropdown-item py-3">
                                    <small className="float-end text-muted ps-2">2 hrs ago</small>
                                    <div className="media">
                                        <div className="avatar-md bg-soft-primary">
                                            <i className="ti ti-users" />
                                        </div>
                                        <div className="media-body align-self-center ms-2 text-truncate">
                                            <h6 className="my-0 fw-normal text-dark">
                                                Payment Successfull
                                            </h6>
                                            <small className="text-muted mb-0">
                                                Dummy text of the printing.
                                            </small>
                                        </div>
                                        {/*end media-body*/}
                                    </div>
                                    {/*end media*/}
                                </a>
                                {/*end-item*/}
                            </div>
                            {/* All*/}
                            <a
                                href="#"
                                className="dropdown-item text-center text-primary"
                            >
                                View all <i className="fi-arrow-right" />
                            </a>
                        </div>
                    </li>
                    <li className="dropdown">
                        <a
                            className="nav-link dropdown-toggle nav-user"
                            data-bs-toggle="dropdown"
                            href="crm-contacts.html#"
                            role="button"
                            aria-haspopup="false"
                            aria-expanded="false"
                        >
                            <div className="d-flex align-items-center">
                                {/* <img
                                    src="assets/images/users/user-4.jpg"
                                    alt="profile-user"
                                    className="rounded-circle me-2 thumb-sm"
                                /> */}
                                <div>
                                    <small className="d-none d-md-block font-11">{auth.user?.userName}</small>
                                    <span className="d-none d-md-block fw-semibold font-12">
                                        {auth.user?.fullName} <i className="fa-solid fa-caret-down"></i>
                                    </span>
                                </div>
                            </div>
                        </a>
                        <div className="dropdown-menu dropdown-menu-end">
                            <a className="dropdown-item" href="crm-contacts.html#">
                                <i className="ti ti-user font-16 me-1 align-text-bottom" /> Profile
                            </a>
                            <a className="dropdown-item" href="crm-contacts.html#">
                                <i className="ti ti-settings font-16 me-1 align-text-bottom" />{" "}
                                Settings
                            </a>
                            <div className="dropdown-divider mb-0" />
                            <button className="dropdown-item" onClick={logout}>
                                <i className="ti ti-power font-16 me-1 align-text-bottom" /> Logout
                            </button>
                        </div>
                    </li>
                    {/*end topbar-profile*/}
                    <li className="notification-list">
                        <a
                            className="nav-link arrow-none nav-icon offcanvas-btn"
                            href="crm-contacts.html#"
                            data-bs-toggle="offcanvas"
                            data-bs-target="#Appearance"
                            role="button"
                            aria-controls="Rightbar"
                        >
                            <i className="ti ti-settings" />
                        </a>
                    </li>
                </ul>
                {/*end topbar-nav*/}
                <ul className="list-unstyled topbar-nav mb-0">
                    <li>
                        <button
                            className="nav-link button-menu-mobile nav-icon"
                            id="togglemenu"
                            onClick={toggleCollapsedMenu}
                        >
                            <i className="ti ti-menu-2" />
                        </button>
                    </li>
                    <li className="hide-phone app-search">
                        <form role="search" action="crm-contacts.html#" method="get">
                            <input
                                type="search"
                                name="search"
                                className="form-control top-search mb-0"
                                placeholder="Type text..."
                            />
                            <button type="submit">
                                <i className="ti ti-search" />
                            </button>
                        </form>
                    </li>
                </ul>
            </nav>
            {/* end navbar*/}
        </div>
    );
};

export default Navbar;
