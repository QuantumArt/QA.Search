import { useEffect } from 'react';
import { acquireLoading } from '../utils/nprogress';

const Loading = () => {
  useEffect(acquireLoading);
  return null;
};

export default Loading;
