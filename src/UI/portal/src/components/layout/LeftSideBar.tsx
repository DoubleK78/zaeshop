import React from 'react';
import { Link } from 'react-router-dom';

const LeftSideBar: React.FC = () => {
    return (
        <div className="leftbar-tab-menu">
            <div className="main-menu-inner">
                <div className="menu-body navbar-vertical tab-content" data-simplebar="">
                    <div
                        id="MetricaDashboard"
                        className="main-icon-menu-pane tab-pane active"
                        role="tabpanel"
                        aria-labelledby="dasboard-tab"
                    >
                        <div className="title-box">
                            <h6 className="menu-title">Dashboard</h6>
                        </div>
                        <ul className="nav flex-column">
                            <li className="nav-item">
                                <Link className="nav-link" to="/users">
                                    Users Management
                                </Link>
                            </li>
                            <li className="nav-item">
                                <Link className="nav-link" to="/roles">
                                    Role Management
                                </Link>
                            </li>
                            <li className="nav-item">
                                <Link className="nav-link" to="/albums">
                                    Album Management
                                </Link>
                            </li>
                        </ul>
                        {/*end nav*/}
                    </div>
                    {/* end Dashboards */}
                    <div
                        id="MetricaApps"
                        className="main-icon-menu-pane tab-pane"
                        role="tabpanel"
                        aria-labelledby="apps-tab"
                    >
                        <div className="title-box">
                            <h6 className="menu-title">Apps</h6>
                        </div>
                        <div className="collapse navbar-collapse" id="sidebarCollapse">
                            {/* Navigation */}
                            <ul className="navbar-nav">
                                <li className="nav-item">
                                    <a
                                        className="nav-link"
                                        href="crm-contacts.html#sidebarAnalytics"
                                        data-bs-toggle="collapse"
                                        role="button"
                                        aria-expanded="false"
                                        aria-controls="sidebarAnalytics"
                                    >
                                        Analytics
                                    </a>
                                    <div className="collapse " id="sidebarAnalytics">
                                        <ul className="nav flex-column">
                                            <li className="nav-item">
                                                <a href="analytics-customers.html" className="nav-link ">
                                                    Customers
                                                </a>
                                            </li>
                                            {/*end nav-item*/}
                                            <li className="nav-item">
                                                <a href="analytics-reports.html" className="nav-link ">
                                                    Reports
                                                </a>
                                            </li>
                                            {/*end nav-item*/}
                                        </ul>
                                        {/*end nav*/}
                                    </div>
                                    {/*end sidebarAnalytics*/}
                                </li>
                                {/*end nav-item*/}
                                <li className="nav-item">
                                    <a
                                        className="nav-link"
                                        href="crm-contacts.html#sidebarCrypto"
                                        data-bs-toggle="collapse"
                                        role="button"
                                        aria-expanded="false"
                                        aria-controls="sidebarCrypto"
                                    >
                                        Crypto
                                    </a>
                                    <div className="collapse " id="sidebarCrypto">
                                        <ul className="nav flex-column">
                                            <li className="nav-item">
                                                <a className="nav-link" href="crypto-exchange.html">
                                                    Exchange
                                                </a>
                                            </li>
                                            {/*end nav-item*/}
                                            <li className="nav-item">
                                                <a className="nav-link" href="crypto-wallet.html">
                                                    Wallet
                                                </a>
                                            </li>
                                            {/*end nav-item*/}
                                            <li className="nav-item">
                                                <a className="nav-link" href="crypto-news.html">
                                                    Crypto News
                                                </a>
                                            </li>
                                            {/*end nav-item*/}
                                            <li className="nav-item">
                                                <a className="nav-link" href="crypto-ico.html">
                                                    ICO List
                                                </a>
                                            </li>
                                            {/*end nav-item*/}
                                            <li className="nav-item">
                                                <a className="nav-link" href="crypto-settings.html">
                                                    Settings
                                                </a>
                                            </li>
                                            {/*end nav-item*/}
                                        </ul>
                                        {/*end nav*/}
                                    </div>
                                    {/*end sidebarCrypto*/}
                                </li>
                                {/*end nav-item*/}
                                <li className="nav-item">
                                    <a
                                        className="nav-link"
                                        href="crm-contacts.html#sidebarCRM"
                                        data-bs-toggle="collapse"
                                        role="button"
                                        aria-expanded="false"
                                        aria-controls="sidebarCRM"
                                    >
                                        CRM
                                    </a>
                                    <div className="collapse " id="sidebarCRM">
                                        <ul className="nav flex-column">
                                            <li className="nav-item">
                                                <a className="nav-link" href="crm-contacts.html">
                                                    Contacts
                                                </a>
                                            </li>
                                            {/*end nav-item*/}
                                            <li className="nav-item">
                                                <a className="nav-link" href="crm-opportunities.html">
                                                    Opportunities
                                                </a>
                                            </li>
                                            {/*end nav-item*/}
                                            <li className="nav-item">
                                                <a className="nav-link" href="crm-leads.html">
                                                    Leads
                                                </a>
                                            </li>
                                            {/*end nav-item*/}
                                            <li className="nav-item">
                                                <a className="nav-link" href="crm-customers.html">
                                                    Customers
                                                </a>
                                            </li>
                                            {/*end nav-item*/}
                                        </ul>
                                        {/*end nav*/}
                                    </div>
                                    {/*end sidebarCRM*/}
                                </li>
                                {/*end nav-item*/}
                                <li className="nav-item">
                                    <a
                                        className="nav-link"
                                        href="crm-contacts.html#sidebarProjects"
                                        data-bs-toggle="collapse"
                                        role="button"
                                        aria-expanded="false"
                                        aria-controls="sidebarProjects"
                                    >
                                        Projects
                                    </a>
                                    <div className="collapse " id="sidebarProjects">
                                        <ul className="nav flex-column">
                                            <li className="nav-item">
                                                <a className="nav-link" href="projects-clients.html">
                                                    Clients
                                                </a>
                                            </li>
                                            {/*end nav-item*/}
                                            <li className="nav-item">
                                                <a className="nav-link" href="projects-team.html">
                                                    Team
                                                </a>
                                            </li>
                                            {/*end nav-item*/}
                                            <li className="nav-item">
                                                <a className="nav-link" href="projects-project.html">
                                                    Project
                                                </a>
                                            </li>
                                            {/*end nav-item*/}
                                            <li className="nav-item">
                                                <a className="nav-link" href="projects-task.html">
                                                    Task
                                                </a>
                                            </li>
                                            {/*end nav-item*/}
                                            <li className="nav-item">
                                                <a className="nav-link" href="projects-kanban-board.html">
                                                    Kanban Board
                                                </a>
                                            </li>
                                            {/*end nav-item*/}
                                            <li className="nav-item">
                                                <a className="nav-link" href="projects-chat.html">
                                                    Chat
                                                </a>
                                            </li>
                                            {/*end nav-item*/}
                                            <li className="nav-item">
                                                <a className="nav-link" href="projects-users.html">
                                                    Users
                                                </a>
                                            </li>
                                            {/*end nav-item*/}
                                            <li className="nav-item">
                                                <a className="nav-link" href="projects-create.html">
                                                    Project Create
                                                </a>
                                            </li>
                                            {/*end nav-item*/}
                                        </ul>
                                        {/*end nav*/}
                                    </div>
                                    {/*end sidebarProjects*/}
                                </li>
                                {/*end nav-item*/}
                                <li className="nav-item">
                                    <a
                                        className="nav-link"
                                        href="crm-contacts.html#sidebarEcommerce"
                                        data-bs-toggle="collapse"
                                        role="button"
                                        aria-expanded="false"
                                        aria-controls="sidebarEcommerce"
                                    >
                                        Ecommerce
                                    </a>
                                    <div className="collapse " id="sidebarEcommerce">
                                        <ul className="nav flex-column">
                                            <li className="nav-item">
                                                <a className="nav-link" href="ecommerce-products.html">
                                                    Products
                                                </a>
                                            </li>
                                            {/*end nav-item*/}
                                            <li className="nav-item">
                                                <a className="nav-link" href="ecommerce-product-list.html">
                                                    Product List
                                                </a>
                                            </li>
                                            {/*end nav-item*/}
                                            <li className="nav-item">
                                                <a
                                                    className="nav-link"
                                                    href="ecommerce-product-detail.html"
                                                >
                                                    Product Detail
                                                </a>
                                            </li>
                                            {/*end nav-item*/}
                                            <li className="nav-item">
                                                <a className="nav-link" href="ecommerce-cart.html">
                                                    Cart
                                                </a>
                                            </li>
                                            {/*end nav-item*/}
                                            <li className="nav-item">
                                                <a className="nav-link" href="ecommerce-checkout.html">
                                                    Checkout
                                                </a>
                                            </li>
                                            {/*end nav-item*/}
                                        </ul>
                                        {/*end nav*/}
                                    </div>
                                    {/*end sidebarEcommerce*/}
                                </li>
                                {/*end nav-item*/}
                                <li className="nav-item">
                                    <a
                                        className="nav-link"
                                        href="crm-contacts.html#sidebarHelpdesk"
                                        data-bs-toggle="collapse"
                                        role="button"
                                        aria-expanded="false"
                                        aria-controls="sidebarHelpdesk"
                                    >
                                        Helpdesk
                                    </a>
                                    <div className="collapse " id="sidebarHelpdesk">
                                        <ul className="nav flex-column">
                                            <li className="nav-item">
                                                <a className="nav-link" href="helpdesk-teckets.html">
                                                    Tickets
                                                </a>
                                            </li>
                                            {/*end nav-item*/}
                                            <li className="nav-item">
                                                <a className="nav-link" href="helpdesk-reports.html">
                                                    Reports
                                                </a>
                                            </li>
                                            {/*end nav-item*/}
                                            <li className="nav-item">
                                                <a className="nav-link" href="helpdesk-agents.html">
                                                    Agents
                                                </a>
                                            </li>
                                            {/*end nav-item*/}
                                        </ul>
                                        {/*end nav*/}
                                    </div>
                                    {/*end sidebarHelpdesk*/}
                                </li>
                                {/*end nav-item*/}
                                <li className="nav-item">
                                    <a
                                        className="nav-link"
                                        href="crm-contacts.html#sidebarHospital"
                                        data-bs-toggle="collapse"
                                        role="button"
                                        aria-expanded="false"
                                        aria-controls="sidebarHospital"
                                    >
                                        Hospital
                                    </a>
                                    <div className="collapse " id="sidebarHospital">
                                        <ul className="nav flex-column">
                                            <li className="nav-item">
                                                <a
                                                    href="crm-contacts.html#sidebarAppointments "
                                                    className="nav-link"
                                                    data-bs-toggle="collapse"
                                                    role="button"
                                                    aria-expanded="false"
                                                    aria-controls="sidebarAppointments"
                                                >
                                                    Appointments
                                                </a>
                                                <div className="collapse " id="sidebarAppointments">
                                                    <ul className="nav flex-column">
                                                        <li className="nav-item">
                                                            <a
                                                                className="nav-link"
                                                                href="hospital-doctor-shedule.html"
                                                            >
                                                                Dr. Shedule
                                                            </a>
                                                        </li>
                                                        {/*end nav-item*/}
                                                        <li className="nav-item">
                                                            <a
                                                                className="nav-link"
                                                                href="hospital-all-appointments.html"
                                                            >
                                                                All Appointments
                                                            </a>
                                                        </li>
                                                        {/*end nav-item*/}
                                                    </ul>
                                                    {/*end nav*/}
                                                </div>
                                                {/*end sidebarAppointments*/}
                                            </li>
                                            {/*end nav-item*/}
                                            <li className="nav-item">
                                                <a
                                                    href="crm-contacts.html#sidebarDoctors"
                                                    className="nav-link"
                                                    data-bs-toggle="collapse"
                                                    role="button"
                                                    aria-expanded="false"
                                                    aria-controls="sidebarDoctors"
                                                >
                                                    Doctors
                                                </a>
                                                <div className="collapse" id="sidebarDoctors">
                                                    <ul className="nav flex-column">
                                                        <li className="nav-item">
                                                            <a
                                                                className="nav-link"
                                                                href="hospital-all-doctors.html"
                                                            >
                                                                All Doctors
                                                            </a>
                                                        </li>
                                                        {/*end nav-item*/}
                                                        <li className="nav-item">
                                                            <a
                                                                className="nav-link"
                                                                href="hospital-add-doctor.html"
                                                            >
                                                                Add Doctor
                                                            </a>
                                                        </li>
                                                        {/*end nav-item*/}
                                                        <li className="nav-item">
                                                            <a
                                                                className="nav-link"
                                                                href="hospital-doctor-edit.html"
                                                            >
                                                                Doctor Edit
                                                            </a>
                                                        </li>
                                                        {/*end nav-item*/}
                                                        <li className="nav-item">
                                                            <a
                                                                className="nav-link"
                                                                href="hospital-doctor-profile.html"
                                                            >
                                                                Doctor Profile
                                                            </a>
                                                        </li>
                                                        {/*end nav-item*/}
                                                    </ul>
                                                    {/*end nav*/}
                                                </div>
                                                {/*end sidebarDoctors*/}
                                            </li>
                                            {/*end nav-item*/}
                                            <li className="nav-item">
                                                <a
                                                    href="crm-contacts.html#sidebarPatients"
                                                    className="nav-link"
                                                    data-bs-toggle="collapse"
                                                    role="button"
                                                    aria-expanded="false"
                                                    aria-controls="sidebarPatients"
                                                >
                                                    Patients
                                                </a>
                                                <div className="collapse" id="sidebarPatients">
                                                    <ul className="nav flex-column">
                                                        <li className="nav-item">
                                                            <a
                                                                className="nav-link"
                                                                href="hospital-all-patients.html"
                                                            >
                                                                All Patients
                                                            </a>
                                                        </li>
                                                        {/*end nav-item*/}
                                                        <li className="nav-item">
                                                            <a
                                                                className="nav-link"
                                                                href="hospital-add-patient.html"
                                                            >
                                                                Add Patient
                                                            </a>
                                                        </li>
                                                        {/*end nav-item*/}
                                                        <li className="nav-item">
                                                            <a
                                                                className="nav-link"
                                                                href="hospital-patient-edit.html"
                                                            >
                                                                Patient Edit
                                                            </a>
                                                        </li>
                                                        {/*end nav-item*/}
                                                        <li className="nav-item">
                                                            <a
                                                                className="nav-link"
                                                                href="hospital-patient-profile.html"
                                                            >
                                                                Patient Profile
                                                            </a>
                                                        </li>
                                                        {/*end nav-item*/}
                                                    </ul>
                                                    {/*end nav*/}
                                                </div>
                                                {/*end sidebarPatients*/}
                                            </li>
                                            {/*end nav-item*/}
                                            <li className="nav-item">
                                                <a
                                                    href="crm-contacts.html#sidebarPayments"
                                                    className="nav-link"
                                                    data-bs-toggle="collapse"
                                                    role="button"
                                                    aria-expanded="false"
                                                    aria-controls="sidebarPayments"
                                                >
                                                    Payments
                                                </a>
                                                <div className="collapse" id="sidebarPayments">
                                                    <ul className="nav flex-column">
                                                        <li className="nav-item">
                                                            <a
                                                                className="nav-link"
                                                                href="hospital-all-payments.html"
                                                            >
                                                                All Payments
                                                            </a>
                                                        </li>
                                                        {/*end nav-item*/}
                                                        <li className="nav-item">
                                                            <a
                                                                className="nav-link"
                                                                href="hospital-payment-invoice.html"
                                                            >
                                                                Payment Invoice
                                                            </a>
                                                        </li>
                                                        {/*end nav-item*/}
                                                        <li className="nav-item">
                                                            <a
                                                                className="nav-link"
                                                                href="hospital-cashless-payments.html"
                                                            >
                                                                Cashless Payments
                                                            </a>
                                                        </li>
                                                        {/*end nav-item*/}
                                                    </ul>
                                                    {/*end nav*/}
                                                </div>
                                                {/*end sidebarPayments*/}
                                            </li>
                                            {/*end nav-item*/}
                                            <li className="nav-item">
                                                <a
                                                    href="crm-contacts.html#sidebarStaff"
                                                    className="nav-link"
                                                    data-bs-toggle="collapse"
                                                    role="button"
                                                    aria-expanded="false"
                                                    aria-controls="sidebarStaff"
                                                >
                                                    Staff
                                                </a>
                                                <div className="collapse" id="sidebarStaff">
                                                    <ul className="nav flex-column">
                                                        <li className="nav-item">
                                                            <a
                                                                className="nav-link"
                                                                href="hospital-all-staff.html"
                                                            >
                                                                All Staff
                                                            </a>
                                                        </li>
                                                        {/*end nav-item*/}
                                                        <li className="nav-item">
                                                            <a
                                                                className="nav-link"
                                                                href="hospital-add-member.html"
                                                            >
                                                                Add Member
                                                            </a>
                                                        </li>
                                                        {/*end nav-item*/}
                                                        <li className="nav-item">
                                                            <a
                                                                className="nav-link"
                                                                href="hospital-edit-member.html"
                                                            >
                                                                Edit Member
                                                            </a>
                                                        </li>
                                                        {/*end nav-item*/}
                                                        <li className="nav-item">
                                                            <a
                                                                className="nav-link"
                                                                href="hospital-member-profile.html"
                                                            >
                                                                Member Profile
                                                            </a>
                                                        </li>
                                                        {/*end nav-item*/}
                                                        <li className="nav-item">
                                                            <a className="nav-link" href="hospital-salary.html">
                                                                Staff Salary
                                                            </a>
                                                        </li>
                                                        {/*end nav-item*/}
                                                    </ul>
                                                    {/*end nav*/}
                                                </div>
                                                {/*end sidebarStaff*/}
                                            </li>
                                            {/*end nav-item*/}
                                            <li className="nav-item">
                                                <a
                                                    href="crm-contacts.html#sidebarGeneral"
                                                    className="nav-link"
                                                    data-bs-toggle="collapse"
                                                    role="button"
                                                    aria-expanded="false"
                                                    aria-controls="sidebarGeneral"
                                                >
                                                    General
                                                </a>
                                                <div className="collapse" id="sidebarGeneral">
                                                    <ul className="nav flex-column">
                                                        <li className="nav-item">
                                                            <a
                                                                className="nav-link"
                                                                href="hospital-all-rooms.html"
                                                            >
                                                                Room Allotments
                                                            </a>
                                                        </li>
                                                        {/*end nav-item*/}
                                                        <li className="nav-item">
                                                            <a className="nav-link" href="hospital-expenses.html">
                                                                Expenses Report
                                                            </a>
                                                        </li>
                                                        {/*end nav-item*/}
                                                        <li className="nav-item">
                                                            <a
                                                                className="nav-link"
                                                                href="hospital-departments.html"
                                                            >
                                                                Departments
                                                            </a>
                                                        </li>
                                                        {/*end nav-item*/}
                                                        <li className="nav-item">
                                                            <a
                                                                className="nav-link"
                                                                href="hospital-insurance-company.html"
                                                            >
                                                                Insurance Co.
                                                            </a>
                                                        </li>
                                                        {/*end nav-item*/}
                                                        <li className="nav-item">
                                                            <a className="nav-link" href="hospital-events.html">
                                                                Events
                                                            </a>
                                                        </li>
                                                        {/*end nav-item*/}
                                                        <li className="nav-item">
                                                            <a className="nav-link" href="hospital-leaves.html">
                                                                Leaves
                                                            </a>
                                                        </li>
                                                        {/*end nav-item*/}
                                                        <li className="nav-item">
                                                            <a className="nav-link" href="hospital-holidays.html">
                                                                Holidays
                                                            </a>
                                                        </li>
                                                        {/*end nav-item*/}
                                                        <li className="nav-item">
                                                            <a
                                                                className="nav-link"
                                                                href="hospital-attendance.html"
                                                            >
                                                                Attendance
                                                            </a>
                                                        </li>
                                                        {/*end nav-item*/}
                                                        <li className="nav-item">
                                                            <a className="nav-link" href="hospital-chat.html">
                                                                Chat
                                                            </a>
                                                        </li>
                                                        {/*end nav-item*/}
                                                    </ul>
                                                    {/*end nav*/}
                                                </div>
                                                {/*end sidebarGeneral*/}
                                            </li>
                                            {/*end nav-item*/}
                                        </ul>
                                        {/*end nav*/}
                                    </div>
                                    {/*end sidebarHospital*/}
                                </li>
                                {/*end nav-item*/}
                                <li className="nav-item">
                                    <a
                                        className="nav-link"
                                        href="crm-contacts.html#sidebarEmail"
                                        data-bs-toggle="collapse"
                                        role="button"
                                        aria-expanded="false"
                                        aria-controls="sidebarEmail"
                                    >
                                        Email
                                    </a>
                                    <div className="collapse " id="sidebarEmail">
                                        <ul className="nav flex-column">
                                            <li className="nav-item">
                                                <a className="nav-link" href="apps-email-inbox.html">
                                                    Inbox
                                                </a>
                                            </li>
                                            {/*end nav-item*/}
                                            <li className="nav-item">
                                                <a className="nav-link" href="apps-email-read.html">
                                                    Read Email
                                                </a>
                                            </li>
                                            {/*end nav-item*/}
                                        </ul>
                                        {/*end nav*/}
                                    </div>
                                    {/*end sidebarEmail*/}
                                </li>
                                {/*end nav-item*/}
                                <li className="nav-item">
                                    <a className="nav-link" href="apps-chat.html">
                                        Chat
                                    </a>
                                </li>
                                {/*end nav-item*/}
                                <li className="nav-item">
                                    <a className="nav-link" href="apps-contact-list.html">
                                        Contact List
                                    </a>
                                </li>
                                {/*end nav-item*/}
                                <li className="nav-item">
                                    <a className="nav-link" href="apps-calendar.html">
                                        Calendar
                                    </a>
                                </li>
                                {/*end nav-item*/}
                                <li className="nav-item">
                                    <a className="nav-link" href="apps-invoice.html">
                                        Invoice
                                    </a>
                                </li>
                                {/*end nav-item*/}
                            </ul>
                            {/*end navbar-nav-*/}
                        </div>
                        {/*end sidebarCollapse*/}
                    </div>
                    {/* end Crypto */}
                    <div
                        id="MetricaUikit"
                        className="main-icon-menu-pane  tab-pane"
                        role="tabpanel"
                        aria-labelledby="uikit-tab"
                    >
                        <div className="title-box">
                            <h6 className="menu-title">UI Kit</h6>
                        </div>
                        <div className="collapse navbar-collapse" id="sidebarCollapse_2">
                            {/* Navigation */}
                            <ul className="navbar-nav">
                                <li className="nav-item">
                                    <a
                                        className="nav-link"
                                        href="crm-contacts.html#sidebarElements"
                                        data-bs-toggle="collapse"
                                        role="button"
                                        aria-expanded="false"
                                        aria-controls="sidebarElements"
                                    >
                                        UI Elements
                                    </a>
                                    <div className="collapse " id="sidebarElements">
                                        <ul className="nav flex-column">
                                            <li className="nav-item">
                                                <a className="nav-link" href="ui-alerts.html">
                                                    Alerts
                                                </a>
                                            </li>
                                            {/*end nav-item*/}
                                            <li className="nav-item">
                                                <a className="nav-link" href="ui-avatar.html">
                                                    Avatar
                                                </a>
                                            </li>
                                            {/*end nav-item*/}
                                            <li className="nav-item">
                                                <a className="nav-link" href="ui-buttons.html">
                                                    Buttons
                                                </a>
                                            </li>
                                            {/*end nav-item*/}
                                            <li className="nav-item">
                                                <a className="nav-link" href="ui-badges.html">
                                                    Badges
                                                </a>
                                            </li>
                                            {/*end nav-item*/}
                                            <li className="nav-item">
                                                <a className="nav-link" href="ui-cards.html">
                                                    Cards
                                                </a>
                                            </li>
                                            {/*end nav-item*/}
                                            <li className="nav-item">
                                                <a className="nav-link" href="ui-carousels.html">
                                                    Carousels
                                                </a>
                                            </li>
                                            {/*end nav-item*/}
                                            <li className="nav-item">
                                                <a className="nav-link" href="ui-dropdowns.html">
                                                    Dropdowns
                                                </a>
                                            </li>
                                            {/*end nav-item*/}
                                            <li className="nav-item">
                                                <a className="nav-link" href="ui-grids.html">
                                                    Grids
                                                </a>
                                            </li>
                                            {/*end nav-item*/}
                                            <li className="nav-item">
                                                <a className="nav-link" href="ui-images.html">
                                                    Images
                                                </a>
                                            </li>
                                            {/*end nav-item*/}
                                            <li className="nav-item">
                                                <a className="nav-link" href="ui-list.html">
                                                    List
                                                </a>
                                            </li>
                                            {/*end nav-item*/}
                                            <li className="nav-item">
                                                <a className="nav-link" href="ui-modals.html">
                                                    Modals
                                                </a>
                                            </li>
                                            {/*end nav-item*/}
                                            <li className="nav-item">
                                                <a className="nav-link" href="ui-navs.html">
                                                    Navs
                                                </a>
                                            </li>
                                            {/*end nav-item*/}
                                            <li className="nav-item">
                                                <a className="nav-link" href="ui-navbar.html">
                                                    Navbar
                                                </a>
                                            </li>
                                            {/*end nav-item*/}
                                            <li className="nav-item">
                                                <a className="nav-link" href="ui-paginations.html">
                                                    Paginations
                                                </a>
                                            </li>
                                            {/*end nav-item*/}
                                            <li className="nav-item">
                                                <a className="nav-link" href="ui-popover-tooltips.html">
                                                    Popover &amp; Tooltips
                                                </a>
                                            </li>
                                            {/*end nav-item*/}
                                            <li className="nav-item">
                                                <a className="nav-link" href="ui-progress.html">
                                                    Progress
                                                </a>
                                            </li>
                                            {/*end nav-item*/}
                                            <li className="nav-item">
                                                <a className="nav-link" href="ui-spinners.html">
                                                    Spinners
                                                </a>
                                            </li>
                                            {/*end nav-item*/}
                                            <li className="nav-item">
                                                <a className="nav-link" href="ui-tabs-accordions.html">
                                                    Tabs &amp; Accordions
                                                </a>
                                            </li>
                                            {/*end nav-item*/}
                                            <li className="nav-item">
                                                <a className="nav-link" href="ui-typography.html">
                                                    Typography
                                                </a>
                                            </li>
                                            {/*end nav-item*/}
                                            <li className="nav-item">
                                                <a className="nav-link" href="ui-videos.html">
                                                    Videos
                                                </a>
                                            </li>
                                            {/*end nav-item*/}
                                        </ul>
                                        {/*end nav*/}
                                    </div>
                                    {/*end sidebarElements*/}
                                </li>
                                {/*end nav-item*/}
                                <li className="nav-item">
                                    <a
                                        className="nav-link"
                                        href="crm-contacts.html#sidebarAdvancedUI"
                                        data-bs-toggle="collapse"
                                        role="button"
                                        aria-expanded="false"
                                        aria-controls="sidebarAdvancedUI"
                                    >
                                        Advanced UI
                                    </a>
                                    <div className="collapse " id="sidebarAdvancedUI">
                                        <ul className="nav flex-column">
                                            <li className="nav-item">
                                                <a className="nav-link" href="advanced-animation.html">
                                                    Animation
                                                </a>
                                            </li>
                                            {/*end nav-item*/}
                                            <li className="nav-item">
                                                <a className="nav-link" href="advanced-clipboard.html">
                                                    Clip Board
                                                </a>
                                            </li>
                                            {/*end nav-item*/}
                                            <li className="nav-item">
                                                <a className="nav-link" href="advanced-dragula.html">
                                                    Dragula
                                                </a>
                                            </li>
                                            {/*end nav-item*/}
                                            <li className="nav-item">
                                                <a className="nav-link" href="advanced-files.html">
                                                    File Manager
                                                </a>
                                            </li>
                                            {/*end nav-item*/}
                                            <li className="nav-item">
                                                <a className="nav-link" href="advanced-highlight.html">
                                                    Highlight
                                                </a>
                                            </li>
                                            {/*end nav-item*/}
                                            <li className="nav-item">
                                                <a className="nav-link" href="advanced-rangeslider.html">
                                                    Range Slider
                                                </a>
                                            </li>
                                            {/*end nav-item*/}
                                            <li className="nav-item">
                                                <a className="nav-link" href="advanced-ratings.html">
                                                    Ratings
                                                </a>
                                            </li>
                                            {/*end nav-item*/}
                                            <li className="nav-item">
                                                <a className="nav-link" href="advanced-ribbons.html">
                                                    Ribbons
                                                </a>
                                            </li>
                                            {/*end nav-item*/}
                                            <li className="nav-item">
                                                <a className="nav-link" href="advanced-sweetalerts.html">
                                                    Sweet Alerts
                                                </a>
                                            </li>
                                            {/*end nav-item*/}
                                            <li className="nav-item">
                                                <a className="nav-link" href="advanced-toasts.html">
                                                    Toasts
                                                </a>
                                            </li>
                                            {/*end nav-item*/}
                                        </ul>
                                        {/*end nav*/}
                                    </div>
                                    {/*end sidebarAdvancedUI*/}
                                </li>
                                {/*end nav-item*/}
                                <li className="nav-item">
                                    <a
                                        className="nav-link"
                                        href="crm-contacts.html#sidebarForms"
                                        data-bs-toggle="collapse"
                                        role="button"
                                        aria-expanded="false"
                                        aria-controls="sidebarForms"
                                    >
                                        Forms
                                    </a>
                                    <div className="collapse " id="sidebarForms">
                                        <ul className="nav flex-column">
                                            <li className="nav-item">
                                                <a className="nav-link" href="forms-elements.html">
                                                    Basic Elements
                                                </a>
                                            </li>
                                            {/*end nav-item*/}
                                            <li className="nav-item">
                                                <a className="nav-link" href="forms-advanced.html">
                                                    Advance Elements
                                                </a>
                                            </li>
                                            {/*end nav-item*/}
                                            <li className="nav-item">
                                                <a className="nav-link" href="forms-validation.html">
                                                    Validation
                                                </a>
                                            </li>
                                            {/*end nav-item*/}
                                            <li className="nav-item">
                                                <a className="nav-link" href="forms-wizard.html">
                                                    Wizard
                                                </a>
                                            </li>
                                            {/*end nav-item*/}
                                            <li className="nav-item">
                                                <a className="nav-link" href="forms-editors.html">
                                                    Editors
                                                </a>
                                            </li>
                                            {/*end nav-item*/}
                                            <li className="nav-item">
                                                <a className="nav-link" href="forms-uploads.html">
                                                    File Upload
                                                </a>
                                            </li>
                                            {/*end nav-item*/}
                                            <li className="nav-item">
                                                <a className="nav-link" href="forms-img-crop.html">
                                                    Image Crop
                                                </a>
                                            </li>
                                            {/*end nav-item*/}
                                        </ul>
                                        {/*end nav*/}
                                    </div>
                                    {/*end sidebarForms*/}
                                </li>
                                {/*end nav-item*/}
                                <li className="nav-item">
                                    <a
                                        className="nav-link"
                                        href="crm-contacts.html#sidebarCharts"
                                        data-bs-toggle="collapse"
                                        role="button"
                                        aria-expanded="false"
                                        aria-controls="sidebarCharts"
                                    >
                                        Charts
                                    </a>
                                    <div className="collapse " id="sidebarCharts">
                                        <ul className="nav flex-column">
                                            <li className="nav-item">
                                                <a className="nav-link" href="charts-apex.html">
                                                    Apex
                                                </a>
                                            </li>
                                            {/*end nav-item*/}
                                            <li className="nav-item">
                                                <a className="nav-link" href="charts-justgage.html">
                                                    JustGage
                                                </a>
                                            </li>
                                            {/*end nav-item*/}
                                            <li className="nav-item">
                                                <a className="nav-link" href="charts-chartjs.html">
                                                    Chartjs
                                                </a>
                                            </li>
                                            {/*end nav-item*/}
                                            <li className="nav-item">
                                                <a className="nav-link" href="charts-toast-ui.html">
                                                    Toast
                                                </a>
                                            </li>
                                            {/*end nav-item*/}
                                        </ul>
                                        {/*end nav*/}
                                    </div>
                                    {/*end sidebarCharts*/}
                                </li>
                                {/*end nav-item*/}
                                <li className="nav-item">
                                    <a
                                        className="nav-link"
                                        href="crm-contacts.html#sidebarTables"
                                        data-bs-toggle="collapse"
                                        role="button"
                                        aria-expanded="false"
                                        aria-controls="sidebarTables"
                                    >
                                        Tables
                                    </a>
                                    <div className="collapse " id="sidebarTables">
                                        <ul className="nav flex-column">
                                            <li className="nav-item">
                                                <a className="nav-link" href="tables-basic.html">
                                                    Basic
                                                </a>
                                            </li>
                                            {/*end nav-item*/}
                                            <li className="nav-item">
                                                <a className="nav-link" href="tables-datatable.html">
                                                    Datatables
                                                </a>
                                            </li>
                                            {/*end nav-item*/}
                                            <li className="nav-item">
                                                <a className="nav-link" href="tables-editable.html">
                                                    Editable
                                                </a>
                                            </li>
                                            {/*end nav-item*/}
                                        </ul>
                                        {/*end nav*/}
                                    </div>
                                    {/*end sidebarTables*/}
                                </li>
                                {/*end nav-item*/}
                                <li className="nav-item">
                                    <a
                                        className="nav-link"
                                        href="crm-contacts.html#sidebarIcons"
                                        data-bs-toggle="collapse"
                                        role="button"
                                        aria-expanded="false"
                                        aria-controls="sidebarIcons"
                                    >
                                        Icons
                                    </a>
                                    <div className="collapse " id="sidebarIcons">
                                        <ul className="nav flex-column">
                                            <li className="nav-item">
                                                <a className="nav-link" href="icons-materialdesign.html">
                                                    Material Design
                                                </a>
                                            </li>
                                            {/*end nav-item*/}
                                            <li className="nav-item">
                                                <a className="nav-link" href="icons-fontawesome.html">
                                                    Font awesome
                                                </a>
                                            </li>
                                            {/*end nav-item*/}
                                            <li className="nav-item">
                                                <a className="nav-link" href="icons-tabler.html">
                                                    Tabler
                                                </a>
                                            </li>
                                            {/*end nav-item*/}
                                            <li className="nav-item">
                                                <a className="nav-link" href="icons-feather.html">
                                                    Feather
                                                </a>
                                            </li>
                                            {/*end nav-item*/}
                                        </ul>
                                        {/*end nav*/}
                                    </div>
                                    {/*end sidebarIcons*/}
                                </li>
                                {/*end nav-item*/}
                                <li className="nav-item">
                                    <a
                                        className="nav-link"
                                        href="crm-contacts.html#sidebarMaps"
                                        data-bs-toggle="collapse"
                                        role="button"
                                        aria-expanded="false"
                                        aria-controls="sidebarMaps"
                                    >
                                        Maps
                                    </a>
                                    <div className="collapse " id="sidebarMaps">
                                        <ul className="nav flex-column">
                                            <li className="nav-item">
                                                <a className="nav-link" href="maps-google.html">
                                                    Google Maps
                                                </a>
                                            </li>
                                            {/*end nav-item*/}
                                            <li className="nav-item">
                                                <a className="nav-link" href="maps-leaflet.html">
                                                    Leaflet Maps
                                                </a>
                                            </li>
                                            {/*end nav-item*/}
                                            <li className="nav-item">
                                                <a className="nav-link" href="maps-vector.html">
                                                    Vector Maps
                                                </a>
                                            </li>
                                            {/*end nav-item*/}
                                        </ul>
                                        {/*end nav*/}
                                    </div>
                                    {/*end sidebarMaps*/}
                                </li>
                                {/*end nav-item*/}
                                <li className="nav-item">
                                    <a
                                        className="nav-link"
                                        href="crm-contacts.html#sidebarEmailTemplates"
                                        data-bs-toggle="collapse"
                                        role="button"
                                        aria-expanded="false"
                                        aria-controls="sidebarEmailTemplates"
                                    >
                                        Email Templates
                                    </a>
                                    <div className="collapse " id="sidebarEmailTemplates">
                                        <ul className="nav flex-column">
                                            <li className="nav-item">
                                                <a className="nav-link" href="email-templates-basic.html">
                                                    Basic Action Email
                                                </a>
                                            </li>
                                            {/*end nav-item*/}
                                            <li className="nav-item">
                                                <a className="nav-link" href="email-templates-alert.html">
                                                    Alert Email
                                                </a>
                                            </li>
                                            {/*end nav-item*/}
                                            <li className="nav-item">
                                                <a className="nav-link" href="email-templates-billing.html">
                                                    Billing Email
                                                </a>
                                            </li>
                                            {/*end nav-item*/}
                                        </ul>
                                        {/*end nav*/}
                                    </div>
                                    {/*end sidebarEmailTemplates*/}
                                </li>
                                {/*end nav-item*/}
                            </ul>
                            {/*end navbar-nav-*/}
                        </div>
                        {/*end sidebarCollapse_2*/}
                    </div>
                    {/* end Others */}
                    <div
                        id="MetricaPages"
                        className="main-icon-menu-pane tab-pane"
                        role="tabpanel"
                        aria-labelledby="pages-tab"
                    >
                        <div className="title-box">
                            <h6 className="menu-title">Pages</h6>
                        </div>
                        <ul className="nav flex-column">
                            <li className="nav-item">
                                <a className="nav-link" href="pages-profile.html">
                                    Profile
                                </a>
                            </li>
                            {/*end nav-item*/}
                            <li className="nav-item">
                                <a className="nav-link" href="pages-tour.html">
                                    Tour
                                </a>
                            </li>
                            {/*end nav-item*/}
                            <li className="nav-item">
                                <a className="nav-link" href="pages-timeline.html">
                                    Timeline
                                </a>
                            </li>
                            {/*end nav-item*/}
                            <li className="nav-item">
                                <a className="nav-link" href="pages-treeview.html">
                                    Treeview
                                </a>
                            </li>
                            {/*end nav-item*/}
                            <li className="nav-item">
                                <a className="nav-link" href="pages-starter.html">
                                    Starter Page
                                </a>
                            </li>
                            {/*end nav-item*/}
                            <li className="nav-item">
                                <a className="nav-link" href="pages-pricing.html">
                                    Pricing
                                </a>
                            </li>
                            {/*end nav-item*/}
                            <li className="nav-item">
                                <a className="nav-link" href="pages-blogs.html">
                                    Blogs
                                </a>
                            </li>
                            {/*end nav-item*/}
                            <li className="nav-item">
                                <a className="nav-link" href="pages-faq.html">
                                    FAQs
                                </a>
                            </li>
                            {/*end nav-item*/}
                            <li className="nav-item">
                                <a className="nav-link" href="pages-gallery.html">
                                    Gallery
                                </a>
                            </li>
                            {/*end nav-item*/}
                        </ul>
                        {/*end nav*/}
                    </div>
                    {/* end Pages */}
                    <div
                        id="MetricaAuthentication"
                        className="main-icon-menu-pane tab-pane"
                        role="tabpanel"
                        aria-labelledby="authentication-tab"
                    >
                        <div className="title-box">
                            <h6 className="menu-title">Authentication</h6>
                        </div>
                        <ul className="nav flex-column">
                            <li className="nav-item">
                                <a className="nav-link" href="auth-login.html">
                                    Log in
                                </a>
                            </li>
                            {/*end nav-item*/}
                            <li className="nav-item">
                                <a className="nav-link" href="auth-login-alt.html">
                                    Log in alt
                                </a>
                            </li>
                            {/*end nav-item*/}
                            <li className="nav-item">
                                <a className="nav-link" href="auth-register.html">
                                    Register
                                </a>
                            </li>
                            {/*end nav-item*/}
                            <li className="nav-item">
                                <a className="nav-link" href="auth-register-alt.html">
                                    Register-alt
                                </a>
                            </li>
                            {/*end nav-item*/}
                            <li className="nav-item">
                                <a className="nav-link" href="auth-recover-pw.html">
                                    Re-Password
                                </a>
                            </li>
                            {/*end nav-item*/}
                            <li className="nav-item">
                                <a className="nav-link" href="auth-recover-pw-alt.html">
                                    Re-Password-alt
                                </a>
                            </li>
                            {/*end nav-item*/}
                            <li className="nav-item">
                                <a className="nav-link" href="auth-lock-screen.html">
                                    Lock Screen
                                </a>
                            </li>
                            {/*end nav-item*/}
                            <li className="nav-item">
                                <a className="nav-link" href="auth-lock-screen-alt.html">
                                    Lock Screen-alt
                                </a>
                            </li>
                            {/*end nav-item*/}
                            <li className="nav-item">
                                <a className="nav-link" href="auth-404.html">
                                    Error 404
                                </a>
                            </li>
                            {/*end nav-item*/}
                            <li className="nav-item">
                                <a className="nav-link" href="auth-404-alt.html">
                                    Error 404-alt
                                </a>
                            </li>
                            {/*end nav-item*/}
                            <li className="nav-item">
                                <a className="nav-link" href="auth-500.html">
                                    Error 500
                                </a>
                            </li>
                            {/*end nav-item*/}
                            <li className="nav-item">
                                <a className="nav-link" href="auth-500-alt.html">
                                    Error 500-alt
                                </a>
                            </li>
                            {/*end nav-item*/}
                        </ul>
                        {/*end nav*/}
                    </div>
                    {/* end Authentication*/}
                </div>
                {/*end menu-body*/}
            </div>
            {/* end main-menu-inner*/}
        </div>
    );
}

export default LeftSideBar;