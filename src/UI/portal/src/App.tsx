import React from 'react';
import AppRoute from './routes/AppRoute';
import { store } from './store/index';
import { Provider } from 'react-redux';
import './utils/i18n';
import './utils/filepond'
import 'react-toastify/dist/ReactToastify.css';

const App: React.FC = () => {
  return (
    <Provider store={store}>
      <AppRoute />
    </Provider>
  );
}

export default App;