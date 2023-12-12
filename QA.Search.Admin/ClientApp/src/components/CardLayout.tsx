import React from 'react';
import { Card, Elevation } from '@blueprintjs/core';

export default ({ children }) => {
  return (
      <div className='card-layout-flex'>
        <div className='flex-basis-three-block'>
          <Card elevation={Elevation.TWO}>
            {children}
          </Card>
        </div>
      </div>
  );
}