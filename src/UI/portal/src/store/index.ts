import { configureStore } from '@reduxjs/toolkit'
import userSlice from './reducers/userSlice'
import authSlice from './reducers/authSlice'
import roleSlice from './reducers/roleSlice'
import albumSlice from './reducers/albumSlice'
import { useDispatch } from 'react-redux'
import albumDetailCollectionSlice from './reducers/albumDetailCollectionSlice'

export const store = configureStore({
  reducer: {
    user: userSlice,
    auth: authSlice,
    role: roleSlice,
    album: albumSlice,
    albumDetailCollection: albumDetailCollectionSlice
  }
})

export type AppDispatch = typeof store.dispatch
export const useAppDispatch: () => AppDispatch = useDispatch // Export a hook that can be reused to resolve types

export type StoreState = ReturnType<typeof store.getState>