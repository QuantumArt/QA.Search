import React from 'react';
import { Row, Col, Grid } from 'react-flexbox-grid';
import { Card, Elevation } from '@blueprintjs/core';

export default ({ children }) => {
  return (
    <Grid fluid>
      <Row center="xs" around="xs" style={{ paddingTop: "100px" }}>
        <Col xs={11} sm={5} md={4} lg={4}>
          <Card elevation={Elevation.TWO}>
            {children}
          </Card>
        </Col>
      </Row>
    </Grid>
  );
}